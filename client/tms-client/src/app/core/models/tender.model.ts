export enum TenderStatus {
  Draft = 'Draft',
  Published = 'Published',
  UnderEvaluation = 'UnderEvaluation',
  Awarded = 'Awarded',
  Closed = 'Closed',
  Cancelled = 'Cancelled'
}

export interface TenderDocument {
  id: number;
  fileName: string;
  filePath: string;
  uploadedAt: string;
}

export interface Tender {
  id: number;
  referenceNumber: string;
  title: string;
  description: string;
  estimatedBudget: number;
  submissionDeadline: string;
  publicationDate: string;
  status: TenderStatus;
  createdByOfficerId: number;
  documents: TenderDocument[];
}

export interface CreateTenderCommand {
  referenceNumber: string;
  title: string;
  description: string;
  estimatedBudget: number;
  submissionDeadline: string;
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}