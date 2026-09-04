export interface Book {
  id: number;
  title: string;
  author: string;
  publishedDate: string;
}

export interface BookUpsertRequest {
  title: string;
  author: string;
  publishedDate: string;
}
