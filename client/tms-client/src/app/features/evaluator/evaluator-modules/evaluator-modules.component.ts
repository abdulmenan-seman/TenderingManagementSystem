import { Component } from '@angular/core'; // Changed from @angular/common
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-evaluator-reports',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-sm space-y-4">
      <h2 class="text-xl font-bold text-slate-800">Evaluation Summaries & Reports</h2>
      <p class="text-xs text-slate-500 font-medium">Aggregated scoring summaries submitted to the Procurement Officer</p>
      <div class="p-4 bg-slate-50 rounded-xl border border-slate-200 text-xs text-slate-600">
        No finalized evaluation reports generated for this session yet.
      </div>
    </div>
  `
})
export class EvaluatorReportsComponent {}

@Component({
  selector: 'app-evaluator-notifications',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-sm space-y-4">
      <h2 class="text-xl font-bold text-slate-800">Notifications & System Alerts</h2>
      <div class="space-y-2">
        <div class="p-3 bg-blue-50 rounded-xl border border-blue-100 text-xs text-blue-900 flex items-center gap-3">
          <mat-icon class="text-blue-600">info</mat-icon>
          <span>You have been assigned as Evaluator for <b>TND-2026-004 (Data Center)</b>.</span>
        </div>
      </div>
    </div>
  `
})
export class EvaluatorNotificationsComponent {}

@Component({
  selector: 'app-evaluator-compliance',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-sm space-y-4">
      <h2 class="text-xl font-bold text-slate-800">Evaluator Code of Ethics & Compliance</h2>
      <div class="text-xs text-slate-600 space-y-2 leading-relaxed">
        <p>1. <b>Impartiality:</b> Every evaluator must evaluate bids based solely on technical merits and pricing criteria.</p>
        <p>2. <b>Conflict of Interest:</b> Declare any personal or financial connections to bidding entities prior to scoring.</p>
        <p>3. <b>Confidentiality:</b> All bidder proprietary information must remain protected within the system audit scope.</p>
      </div>
    </div>
  `
})
export class EvaluatorComplianceComponent {}