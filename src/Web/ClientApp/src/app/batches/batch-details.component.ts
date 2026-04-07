import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

interface BatchDetailsVm {
  id: number;
  batchNumber: string;
  pdrsNumber: string;
  productName: string | null;
  headerTitle: string | null;
  status: string | null;
  releaseDate: string | null;
  retentionDueDate: string | null;
  fileTypeName: string | null;
  locationName: string | null;
  isArchived: boolean;
}

interface AppWorksheetSampleRowVm {
  sampleNumber: string | null;
  test: string | null;
  specification: string | null;
  specificationForInformationOnly: string | null;
  result: string | null;
}

interface WorksheetDisplayRow {
  sampleNumber: string;
  test: string;
  specification: string;
  specificationForInformationOnly: string;
  result: string;
}

interface WorksheetRowGroup {
  title: string;
  rows: WorksheetDisplayRow[];
}

interface WorksheetColumnHeaders {
  sampleNumber: string;
  test: string;
  specification: string;
  specificationForInformationOnly: string;
  result: string;
}

interface AppWorksheetSectionVm {
  name: string;
  comments: string | null;
  samples: AppWorksheetSampleRowVm[];
}

interface AppWorksheetVm {
  sections: AppWorksheetSectionVm[];
}

@Component({
  standalone: false,
  selector: 'app-batch-details',
  templateUrl: './batch-details.component.html',
  styleUrls: ['./batch-details.component.scss']
})
export class BatchDetailsComponent implements OnInit {
  loading = true;
  loadError: string | null = null;
  batch: BatchDetailsVm | null = null;
  worksheet: AppWorksheetVm | null = null;
  readonly buildMarker = 'details-ui-refresh-2026-04-07-1';

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    void this.loadBatch();
  }

  private async loadBatch(): Promise<void> {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.loading = false;
      this.loadError = 'Invalid batch id.';
      return;
    }
    this.loading = true;
    this.loadError = null;
    this.batch = null;
    this.worksheet = null;
    setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        this.loadError = 'Details UI watchdog fired.';
        this.cdr.detectChanges();
      }
    }, 12000);

    try {
      const raw = await this.fetchJson(`/api/batches/${id}`, 10000);
      const vm = this.normalizeDetails(raw);
      if (!vm) {
        this.loadError = 'Unexpected response format for batch details.';
        this.cdr.detectChanges();
        return;
      }
      this.batch = vm;
      const isWi = (vm.fileTypeName ?? '').trim().toUpperCase() === 'WI';
      const worksheetUrl = isWi ? `/api/batches/${id}/wi-worksheet` : `/api/batches/${id}/app-worksheet`;
      const wsRaw = await this.fetchJson(worksheetUrl, 10000);
      this.worksheet = wsRaw as AppWorksheetVm;
      this.cdr.detectChanges();
    } catch {
      this.loadError = 'Could not load batch details.';
      this.batch = null;
      this.cdr.detectChanges();
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  private normalizeDetails(raw: unknown): BatchDetailsVm | null {
    if (!raw || typeof raw !== 'object') {
      return null;
    }

    const obj = raw as Record<string, unknown>;
    const candidate =
      Array.isArray(raw) ? raw[0] :
      (obj['item'] && typeof obj['item'] === 'object' ? obj['item'] : raw);

    if (!candidate || typeof candidate !== 'object') {
      return null;
    }
    return candidate as BatchDetailsVm;
  }

  private async fetchJson(url: string, timeoutMs: number): Promise<unknown> {
    const controller = new AbortController();
    const id = setTimeout(() => controller.abort(), timeoutMs);
    try {
      const res = await fetch(url, {
        method: 'GET',
        credentials: 'include',
        signal: controller.signal
      });
      if (!res.ok) {
        throw new Error(`HTTP ${res.status}`);
      }
      return await res.json();
    } finally {
      clearTimeout(id);
    }
  }

  formatBatchDate(iso: string | null | undefined): string {
    if (!iso) {
      return '—';
    }
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) {
      return iso;
    }
    return d.toLocaleDateString(undefined, {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }

  sectionRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    return section.samples.map(r => ({
      sampleNumber: r.sampleNumber || '—',
      test: r.test || '—',
      specification: r.specification || '—',
      specificationForInformationOnly: r.specificationForInformationOnly || '—',
      result: r.result || '—'
    }));
  }

  sectionHeaders(sectionName: string): WorksheetColumnHeaders {
    if (sectionName === 'UHT Evaporator') {
      return {
        sampleNumber: 'Time (HHMM)',
        test: 'HMI Density (kg/L)',
        specification: 'Total Solids (%)',
        specificationForInformationOnly: 'Viscosity (cP)',
        result: 'Temperature/Torque/Speed'
      };
    }

    if (sectionName === 'Dryer Parameters') {
      return {
        sampleNumber: 'Parameter',
        test: 'UOM',
        specification: 'Pre-Set',
        specificationForInformationOnly: 'Pre-Trial Value',
        result: 'Post-trial Recommended Value'
      };
    }

    if (sectionName === 'Drying and Dryer Log') {
      return {
        sampleNumber: 'Sample/Description',
        test: 'Test/Unit',
        specification: 'Specification/Freq',
        specificationForInformationOnly: 'Reading 1',
        result: 'Readings / Result'
      };
    }

    return {
      sampleNumber: 'Sample #',
      test: 'Test',
      specification: 'Specification',
      specificationForInformationOnly: 'Info Only',
      result: 'Result'
    };
  }

  isWiSection(section: AppWorksheetSectionVm): boolean {
    return section.name.includes('(WI)');
  }

  wiBaseName(section: AppWorksheetSectionVm): string {
    return section.name.replace('(WI)', '').trim();
  }

  wiInstruction(section: AppWorksheetSectionVm): string {
    const name = this.wiBaseName(section);
    if (name === 'PIF' || name === 'WSM') return '4) Observation and Comments';
    if (name === 'PIW') return '3) Observation and Comments';
    if (name === 'Hydration' || name === 'FPT') return '3) Collect sample for pH testing';
    if (name === 'Evaporator') return 'Record time of start of product feed, monitor and record parameter readings every 30 minutes';
    if (name === 'Dryer') return 'Record nozzle orifice and core size used';
    return 'Observation and Comments';
  }

  wiIsSimpleCommentSection(section: AppWorksheetSectionVm): boolean {
    const name = this.wiBaseName(section);
    return name === 'PIF' || name === 'PIW' || name === 'WSM' || name.startsWith('WSV');
  }

  wiIsPhSection(section: AppWorksheetSectionVm): boolean {
    const name = this.wiBaseName(section);
    return name === 'Hydration' || name === 'FPT';
  }

  wiIsEvaporator(section: AppWorksheetSectionVm): boolean {
    return this.wiBaseName(section) === 'Evaporator';
  }

  wiIsDryer(section: AppWorksheetSectionVm): boolean {
    return this.wiBaseName(section) === 'Dryer';
  }

  wiObservation(section: AppWorksheetSectionVm): string {
    if (section.comments && section.comments.trim().length > 0) {
      return section.comments;
    }
    const first = this.sectionRows(section)[0];
    if (!first) return '—';
    if (first.result && first.result !== '—') return first.result;
    if (first.test && first.test !== '—') return first.test;
    return '—';
  }

  wiPhSummaryRow(section: AppWorksheetSectionVm): WorksheetDisplayRow {
    const rows = this.sectionRows(section);
    if (rows.length > 0) {
      return rows[0];
    }
    return {
      sampleNumber: 'pH specification (at 25 degC)',
      test: 'Target pH (at 25 degC)',
      specification: 'pH before correction (at 25 degC)',
      specificationForInformationOnly: 'pH correction needed?',
      result: 'Final pH after correction'
    };
  }

  wiPhTitrationRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section).slice(1);
    if (rows.length > 0) {
      return rows;
    }
    return [
      { sampleNumber: 'Weight of Sample Drawn (g)', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Weight of 5% KOH used (g)', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Weight of Hydration Tank (kg)', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Weight of 5% KOH needed for tank (kg)', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Added?', test: 'Yes/No', specification: 'Yes/No', specificationForInformationOnly: 'Yes/No', result: '—' }
    ];
  }

  wiEvaporatorRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    if (rows.length > 0) return rows;
    return [
      { sampleNumber: 'Parameter 1', test: 'Rec 1', specification: 'Rec 2', specificationForInformationOnly: 'Rec 3', result: 'FPT (Concentrate)' },
      { sampleNumber: 'Parameter 2', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' }
    ];
  }

  wiDryerRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    if (rows.length > 0) return rows;
    return [
      { sampleNumber: 'Orifice Size', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Core Size', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' },
      { sampleNumber: 'Special Instructions / Remarks', test: '—', specification: '—', specificationForInformationOnly: '—', result: '—' }
    ];
  }

  isPifSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'PIF';
  }

  isWsmSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'WSM';
  }

  isPiwSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'PIW';
  }

  isBlendingSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'Blending';
  }

  isWsvSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'WSV Solution';
  }

  isFptSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'FPT';
  }

  isUhtSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'UHT Evaporator';
  }

  isDryerSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'Dryer Parameters';
  }

  isDryingSection(section: AppWorksheetSectionVm): boolean {
    return section.name === 'Drying and Dryer Log';
  }

  blendingPreMixRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(0, Math.min(11, rows.length));
  }

  blendingHydrationGroups(section: AppWorksheetSectionVm): WorksheetRowGroup[] {
    const rows = this.sectionRows(section);
    const hydrationRows = rows.slice(11, 19);
    return [
      { title: 'Blend/Hydration Sample 1', rows: hydrationRows.slice(0, 1) },
      { title: 'Blend/Hydration Sample 2', rows: hydrationRows.slice(1, 2) },
      { title: 'Blend/Hydration Sample 3', rows: hydrationRows.slice(2, 7) },
      { title: 'Blend/Hydration Sample 4', rows: hydrationRows.slice(7, 8) }
    ].filter(g => g.rows.length > 0);
  }

  blendingHtstRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(19, 22);
  }

  fptPreMixRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(0, Math.min(11, rows.length));
  }

  fptHtstRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(11);
  }

  uhtRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    return this.sectionRows(section);
  }

  dryerParamRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(0, Math.min(17, rows.length));
  }

  dryerIptRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const rows = this.sectionRows(section);
    return rows.slice(17);
  }

  dryingInfoRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const infoKeys = new Set(['Orifice Size', 'Core', 'Lance Position', 'Hourly Parameter Water/Product', 'End Time']);
    return this.sectionRows(section).filter(r => infoKeys.has(r.sampleNumber));
  }

  dryingPerformRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    return this.sectionRows(section).filter(r => (r.result || '').includes('MOB:'));
  }

  dryingCollectRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    return this.sectionRows(section).filter(r => r.test === 'Bulk Density');
  }

  dryingLogRows(section: AppWorksheetSectionVm): WorksheetDisplayRow[] {
    const infoRows = new Set(this.dryingInfoRows(section).map(r => r.sampleNumber));
    const performRows = new Set(this.dryingPerformRows(section).map(r => `${r.sampleNumber}|${r.test}`));
    const collectRows = new Set(this.dryingCollectRows(section).map(r => `${r.sampleNumber}|${r.test}`));
    return this.sectionRows(section).filter(r =>
      !infoRows.has(r.sampleNumber) &&
      !performRows.has(`${r.sampleNumber}|${r.test}`) &&
      !collectRows.has(`${r.sampleNumber}|${r.test}`));
  }

  blendHydValue(row: WorksheetDisplayRow, key: 'HYD-1' | 'HYD-2' | 'Result'): string {
    const value = row.result || '';
    if (!value || value === '—') {
      return '—';
    }

    const chunks = value.split('|').map(x => x.trim());
    const prefix = `${key}:`;
    const match = chunks.find(c => c.toLowerCase().startsWith(prefix.toLowerCase()));
    if (!match) {
      return key === 'Result' ? value : '—';
    }

    const out = match.slice(prefix.length).trim();
    return out || '—';
  }

  taggedValue(value: string, key: string): string {
    if (!value || value === '—') {
      return '—';
    }

    const chunks = value.split('|').map(x => x.trim());
    const prefix = `${key}:`;
    const match = chunks.find(c => c.toLowerCase().startsWith(prefix.toLowerCase()));
    if (!match) {
      return '—';
    }

    const out = match.slice(prefix.length).trim();
    return out || '—';
  }
}
