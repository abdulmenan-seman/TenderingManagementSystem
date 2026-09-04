export interface BidDocument {
  id: number;
  documentType: string;
  fileName: string;
  filePath: string;
  uploadedAt: string;
}

export interface Bid {
  id: number;
  tenderId: number;
  supplierId: number;
  financialProposalAmount: number;
  submissionDate: string;
  status: string;
  documents: BidDocument[];
}