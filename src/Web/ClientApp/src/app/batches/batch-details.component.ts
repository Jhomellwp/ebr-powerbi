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
  readonly buildMarker = 'details-rebuild-2026-04-07-1';

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
}
