import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

/** Matches API: ebr_powerbi.Application.Common.Interfaces.BatchListItemDto */
export type BatchKind = 'all' | 'app' | 'wi' | 'app_master';

export interface BatchListItemDto {
  id: number;
  batchNumber: string;
  pdrsNumber: string;
  productName: string | null;
  status: string | null;
  releaseDate: string | null;
  retentionDueDate: string | null;
  fileTypeName: string | null;
}

interface BatchesListVm {
  items: BatchListItemDto[];
}

@Component({
  standalone: false,
  selector: 'app-batch-list',
  templateUrl: './batch-list.component.html',
  styleUrls: ['./batch-list.component.scss']
})
export class BatchListComponent implements OnInit {
  batches: BatchListItemDto[] = [];
  selectedType: BatchKind = 'all';
  selectedPdrs = '';
  selectedStatusFilter = '';
  tableSearch = '';

  pageIndex = 0;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];

  loading = false;
  loadError: string | null = null;
  readonly buildMarker = 'rebuild-2026-04-07-2';

  constructor(private readonly cdr: ChangeDetectorRef) {}

  readonly typeFilterOptions: { value: BatchKind; label: string }[] = [
    { value: 'all', label: 'All batches' },
    { value: 'app', label: 'APP' },
    { value: 'wi', label: 'WI' },
    { value: 'app_master', label: 'APP Master' }
  ];

  get pdrsOptions(): string[] {
    const set = new Set<string>();
    for (const b of this.typeScopedBatches) {
      const p = (b.pdrsNumber ?? '').trim();
      if (p) {
        set.add(p);
      }
    }
    return Array.from(set).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }));
  }

  get statusFilterOptions(): string[] {
    const set = new Set<string>();
    for (const b of this.scopedBatches) {
      const s = (b.status ?? '').trim();
      if (s) {
        set.add(s);
      }
    }
    return Array.from(set).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }));
  }

  get filteredBatches(): BatchListItemDto[] {
    let rows = this.scopedBatches;
    if (this.selectedStatusFilter === '__blank__') {
      rows = rows.filter(b => !(b.status ?? '').trim());
    } else if (this.selectedStatusFilter) {
      rows = rows.filter(b => (b.status ?? '').trim() === this.selectedStatusFilter);
    }

    const q = this.tableSearch.trim().toLowerCase();
    if (!q) {
      return rows;
    }
    return rows.filter(b => {
      const hay = [
        String(b.id),
        b.batchNumber ?? '',
        b.pdrsNumber ?? '',
        b.productName ?? '',
        b.status ?? '',
        b.releaseDate ?? '',
        b.retentionDueDate ?? '',
        b.fileTypeName ?? ''
      ]
        .join(' ')
        .toLowerCase();
      return hay.includes(q);
    });
  }

  get totalFiltered(): number {
    return this.filteredBatches.length;
  }

  get totalPages(): number {
    if (this.totalFiltered === 0) {
      return 1;
    }
    return Math.ceil(this.totalFiltered / this.pageSize);
  }

  get pagedBatches(): BatchListItemDto[] {
    const filtered = this.filteredBatches;
    const start = this.pageIndex * this.pageSize;
    return filtered.slice(start, start + this.pageSize);
  }

  /** Human-readable range for the footer (e.g. "Showing 1–25 of 140"). */
  get tableRangeLabel(): string {
    const total = this.totalFiltered;
    if (total === 0) {
      return '';
    }
    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}–${end} of ${total}`;
  }

  ngOnInit(): void {
    setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        this.loadError = 'UI watchdog: request did not settle in time.';
        this.cdr.detectChanges();
      }
    }, 12000);
    void this.loadBatches();
  }

  onSearchChange(): void {
    this.pageIndex = 0;
  }

  onStatusFilterChange(): void {
    this.pageIndex = 0;
  }

  onPageSizeChange(): void {
    this.pageIndex = 0;
  }

  goPrevPage(): void {
    this.pageIndex = Math.max(0, this.pageIndex - 1);
  }

  goNextPage(): void {
    this.pageIndex = Math.min(this.totalPages - 1, this.pageIndex + 1);
  }

  goFirstPage(): void {
    this.pageIndex = 0;
  }

  goLastPage(): void {
    this.pageIndex = Math.max(0, this.totalPages - 1);
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

  onTypeChange(): void {
    this.selectedPdrs = '';
    this.selectedStatusFilter = '';
    this.tableSearch = '';
    this.pageIndex = 0;
  }

  private batchListUrl(): string {
    return '/api/batches';
  }

  /** API may send `{ items }`, `{ Items }`, or a bare array depending on serializer / client. */
  private normalizeBatchesVm(raw: unknown): BatchesListVm {
    if (Array.isArray(raw)) {
      return { items: raw as BatchListItemDto[] };
    }
    if (raw && typeof raw === 'object') {
      const o = raw as Record<string, unknown>;
      const items = o['items'] ?? o['Items'];
      if (Array.isArray(items)) {
        return { items: items as BatchListItemDto[] };
      }
    }
    return { items: [] };
  }

  private async loadBatches(): Promise<void> {
    this.loading = true;
    this.loadError = null;
    this.batches = [];
    this.pageIndex = 0;
    try {
      const raw = await this.fetchJson(this.batchListUrl(), 10000);
      this.batches = this.normalizeBatchesVm(raw).items ?? [];
      this.tableSearch = '';
      this.selectedStatusFilter = '';
      this.pageIndex = 0;
      this.cdr.detectChanges();
    } catch {
      this.batches = [];
      this.selectedPdrs = '';
      this.tableSearch = '';
      this.selectedStatusFilter = '';
      this.pageIndex = 0;
      this.loadError =
        'Could not load batches. Check that the API is running and the database connection matches EBR.';
      this.cdr.detectChanges();
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
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

  batchRowTrackKey(index: number, b: BatchListItemDto): string {
    return `${b.id}|${b.batchNumber ?? ''}|${b.pdrsNumber ?? ''}|${index}`;
  }

  private get typeScopedBatches(): BatchListItemDto[] {
    if (this.selectedType === 'all') {
      return this.batches;
    }
    return this.batches.filter(b => this.matchesKind(this.selectedType, b.fileTypeName));
  }

  private get scopedBatches(): BatchListItemDto[] {
    const rows = this.typeScopedBatches;
    const p = this.selectedPdrs.trim();
    if (!p) {
      return rows;
    }
    return rows.filter(b => (b.pdrsNumber ?? '').trim() === p);
  }

  private matchesKind(kind: BatchKind, fileTypeName: string | null): boolean {
    const t = (fileTypeName ?? '').trim().toUpperCase();
    if (kind === 'app') {
      return t === 'APP';
    }
    if (kind === 'wi') {
      return t === 'WI';
    }
    if (kind === 'app_master') {
      return t === 'APP_PACKING_MANUAL';
    }
    return true;
  }
}
