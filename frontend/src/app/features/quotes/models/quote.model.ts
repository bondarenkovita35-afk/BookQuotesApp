export interface Quote {
  id: number;
  text: string;
  author: string | null;
}

export interface QuoteUpsertRequest {
  text: string;
  author: string | null;
}
