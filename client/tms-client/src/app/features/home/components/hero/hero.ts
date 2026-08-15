import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './hero.html',
  styleUrls: ['./hero.scss']
})
export class HeroComponent {
  stats = [
    { value: '$1.2B+', label: 'Tenders Processed' },
    { value: '4,500+', label: 'Verified Suppliers' },
    { value: '99.8%', label: 'Compliance Rate' },
    { value: '24/7', label: 'Automated Evaluation' }
  ];
}