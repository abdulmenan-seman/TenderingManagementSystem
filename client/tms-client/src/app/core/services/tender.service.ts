import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, switchMap, forkJoin, of, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Tender,
  TenderDocument,
  CreateTenderCommand,
  PaginatedResult,
  TenderStatus
} from '../models/tender.model';

@Injectable({
  providedIn: 'root'
})
export class TenderService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl || 'http://localhost:5293/api/v1'}/tenders`;

  // Signals for reactive local state management
  tenders = signal<Tender[]>([]);
  selectedTender = signal<Tender | null>(null);
  totalCount = signal<number>(0);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // ─── READ ──────────────────────────────────────────────────────────────────

  /**
   * Fetch paginated tenders with optional status filter.
   * Maps the server's PaginatedTendersDto shape (items, totalCount, ...).
   */
  getTenders(
    pageNumber: number = 1,
    pageSize: number = 10,
    status?: TenderStatus
  ): Observable<PaginatedResult<Tender>> {
    this.loading.set(true);
    this.error.set(null);

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PaginatedResult<Tender>>(this.apiUrl, { params }).pipe(
      tap({
        next: (res) => {
          this.tenders.set(res.items ?? []);
          this.totalCount.set(res.totalCount ?? 0);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err?.error?.errors?.[0] ?? 'Failed to load tenders.');
        }
      })
    );
  }

  /**
   * Get full detail for a single tender by ID.
   */
  getTenderById(id: number): Observable<Tender> {
    this.loading.set(true);
    return this.http.get<Tender>(`${this.apiUrl}/${id}`).pipe(
      tap({
        next: (tender) => {
          this.selectedTender.set(tender);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  // ─── CREATE + UPLOAD ───────────────────────────────────────────────────────

  /**
   * Creates a new Draft tender, then sequentially uploads any attached documents.
   * Returns the created Tender (with documents if any were uploaded).
   */
  createTenderWithDocuments(
    command: CreateTenderCommand,
    files: File[]
  ): Observable<Tender> {
    this.loading.set(true);
    this.error.set(null);

    return this.http.post<Tender>(this.apiUrl, command).pipe(
      switchMap((createdTender) => {
        if (!files || files.length === 0) {
          return of(createdTender);
        }
        // Upload all documents in parallel, then return the tender
        const uploads$ = files.map((file) =>
          this.uploadTenderDocument(createdTender.id, file)
        );
        return forkJoin(uploads$).pipe(
          tap((uploadedDocs) => {
            // Merge uploaded documents into the created tender
            createdTender.documents = [
              ...(createdTender.documents ?? []),
              ...uploadedDocs
            ];
          }),
          switchMap(() => of(createdTender)),
          catchError((err) => {
            this.error.set(
              `Tender created, but its documents could not be uploaded: ${this.getServerError(err)}`
            );
            return of(createdTender);
          })
        );
      }),
      tap({
        next: (tender) => {
          this.tenders.update((items) => [tender, ...items]);
          this.totalCount.update((c) => c + 1);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(this.getServerError(err, 'Failed to create tender.'));
        }
      })
    );
  }

  // ─── UPDATE ────────────────────────────────────────────────────────────────

  /**
   * Updates the core fields of a Draft tender.
   */
  updateTender(id: number, command: CreateTenderCommand): Observable<Tender> {
    this.loading.set(true);
    return this.http.put<Tender>(`${this.apiUrl}/${id}`, command).pipe(
      tap({
        next: (updated) => {
          this.tenders.update((items) =>
            items.map((item) => (item.id === id ? updated : item))
          );
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          const serverError = err?.error?.error || (err?.error?.errors ? (Object.values(err.error.errors) as any[])[0]?.[0] : null) || 'Failed to update tender.';
          this.error.set(serverError);
        }
      })
    );
  }

  // ─── DOCUMENT UPLOAD ──────────────────────────────────────────────────────

  /**
   * Uploads a single document and attaches it to an existing tender.
   * Sends as multipart/form-data.
   */
  uploadTenderDocument(tenderId: number, file: File): Observable<TenderDocument> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<TenderDocument>(`${this.apiUrl}/${tenderId}/documents`, formData);
  }

  downloadTenderDocument(filePath: string, fileName: string): void {
    this.http.get(this.toApiUrl(filePath), { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: (err) => this.error.set(this.getServerError(err, 'Failed to download document.'))
    });
  }

  private toApiUrl(path: string): string {
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    const apiUrl = environment.apiUrl?.replace(/\/$/, '') || 'http://localhost:5293/api/v1';
    if (path.startsWith('/api/')) {
      return `${apiUrl.replace(/\/api\/v1$/, '')}${path}`;
    }
    return `${apiUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }

  private getServerError(err: any, fallback = 'Failed to process request.'): string {
    return err?.error?.error
      || (err?.error?.errors ? (Object.values(err.error.errors) as any[])[0]?.[0] : null)
      || fallback;
  }

  // ─── WORKFLOW ACTIONS ─────────────────────────────────────────────────────

  /**
   * Publishes a Draft tender, making it visible to bidders.
   */
  publishTender(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/publish`, {}).pipe(
      tap({
        next: () => this.updateLocalTenderStatus(id, TenderStatus.Published),
        error: (err) => {
          const serverError = err?.error?.error || 'Failed to publish tender.';
          this.error.set(serverError);
        }
      })
    );
  }

  /**
   * Transitions a Published tender to UnderEvaluation state.
   */
  startEvaluation(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/start-evaluation`, {}).pipe(
      tap(() => this.updateLocalTenderStatus(id, TenderStatus.UnderEvaluation))
    );
  }

  /**
   * Awards a tender under evaluation to the selected winning bid.
   */
  awardTender(id: number, winningBidId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/award`, { winningBidId }).pipe(
      tap(() => this.updateLocalTenderStatus(id, TenderStatus.Awarded))
    );
  }

  /**
   * Closes/Cancels a Published tender.
   */
  closeTender(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/close`, {}).pipe(
      tap(() => this.updateLocalTenderStatus(id, TenderStatus.Closed))
    );
  }

  // ─── DELETE ──────────────────────────────────────────────────────────────

  /**
   * Soft-deletes a Draft tender.
   */
  deleteTender(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.tenders.update((items) => items.filter((t) => t.id !== id));
        this.totalCount.update((c) => Math.max(0, c - 1));
      })
    );
  }

  // ─── HELPERS ─────────────────────────────────────────────────────────────

  private updateLocalTenderStatus(id: number, newStatus: TenderStatus): void {
    this.tenders.update((items) =>
      items.map((tender) =>
        tender.id === id ? { ...tender, status: newStatus } : tender
      )
    );
    const selected = this.selectedTender();
    if (selected && selected.id === id) {
      this.selectedTender.set({ ...selected, status: newStatus });
    }
  }
}