import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

interface Tender {
  id: string;
  code: string;
  title: string;
  category: string;
  budget: string;
  deadline: string;
  bidsCount: number;
  status: 'Open' | 'Closing Soon';
}

@Component({
  selector: 'app-featured-tenders',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './featured-tenders.html',
  styleUrls: ['./featured-tenders.scss']
})
export class FeaturedTendersComponent {
  tenders: Tender[] = [
    {
      id: '1',
      code: 'TND-2026-104',
      title: 'Supply & Delivery of Smart POS Terminals',
      category: 'Hardware & Devices',
      budget: '$280,000',
      deadline: '3 Days left',
      bidsCount: 8,
      status: 'Closing Soon'
    },
    {
      id: '2',
      code: 'TND-2026-109',
      title: 'Automated Inventory & Warehouse AI Integration',
      category: 'Software Development',
      budget: '$520,000',
      deadline: '12 Days left',
      bidsCount: 15,
      status: 'Open'
    },
    {
      id: '3',
      code: 'TND-2026-112',
      title: 'Cold Storage Maintenance & HVAC Overhaul',
      category: 'Infrastructure & Facilities',
      budget: '$195,000',
      deadline: '5 Days left',
      bidsCount: 6,
      status: 'Closing Soon'
    }
  ];
}