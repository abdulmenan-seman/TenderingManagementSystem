import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ProcurementService } from '../../../../core/services/procurement.service';

@Component({
  selector: 'app-procurement-overview',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './procurement-overview.html',
  styleUrls: ['./procurement-overview.scss'],
})
export class ProcurementOverviewComponent implements OnInit {
  protected procurementService = inject(ProcurementService);

  ngOnInit(): void {
    this.procurementService.fetchProcurements().subscribe();
  }
}