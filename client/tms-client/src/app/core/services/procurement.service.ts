import { Injectable, signal, computed } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay, tap } from 'rxjs/operators';

export interface ProcurementItem {
  id: string;
  title: string;
  category: string;
  budget: number;
  status: 'Pending' | 'Approved' | 'Rejected';
  createdAt: Date;
}

@Injectable({
  providedIn: 'root',
})
export class ProcurementService {
  // Signal-based state management
  private itemsSignal = signal<ProcurementItem[]>([]);
  private loadingSignal = signal<boolean>(false);

  // Read-only public signals
  readonly items = this.itemsSignal.asReadonly();
  readonly isLoading = this.loadingSignal.asReadonly();
  
  // Computed stats
  readonly totalBudget = computed(() => 
    this.items().reduce((sum, item) => sum + item.budget, 0)
  );
  readonly pendingCount = computed(() => 
    this.items().filter(item => item.status === 'Pending').length
  );

  private mockData: ProcurementItem[] = [
    { id: 'PR-101', title: 'Office Hardware Upgrade', category: 'IT', budget: 15000, status: 'Approved', createdAt: new Date('2026-08-01') },
    { id: 'PR-102', title: 'Network Infrastructure Expansion', category: 'IT', budget: 32000, status: 'Pending', createdAt: new Date('2026-08-10') },
    { id: 'PR-103', title: 'Stationery Supplies Q3', category: 'Operations', budget: 2500, status: 'Approved', createdAt: new Date('2026-08-15') },
  ];

  fetchProcurements(): Observable<ProcurementItem[]> {
    this.loadingSignal.set(true);
    return of(this.mockData).pipe(
      delay(500),
      tap(items => {
        this.itemsSignal.set(items);
        this.loadingSignal.set(false);
      })
    );
  }

  addProcurement(item: Omit<ProcurementItem, 'id' | 'createdAt'>): void {
    const newItem: ProcurementItem = {
      ...item,
      id: `PR-${Math.floor(100 + Math.random() * 900)}`,
      createdAt: new Date(),
    };
    this.itemsSignal.update(items => [newItem, ...items]);
  }
}