import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface DocumentSummary {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  status: string;
  uploadedAt: string;
}

export interface ValidationResult {
  isValid: boolean;
  documentId: string;
  signedBy: string | null;
  algorithm: string | null;
  certificateThumbprint: string | null;
  signedAt: string | null;
  failureReason: string | null;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private readonly base = `${environment.apiUrl}/documents`;

  constructor(private http: HttpClient) {}

  list() {
    return this.http.get<DocumentSummary[]>(this.base);
  }

  upload(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ id: string }>(this.base, form);
  }

  sign(id: string) {
    return this.http.post<{ signatureId: string }>(
      `${this.base}/${id}/sign`, {}
    );
  }

  validate(id: string) {
    return this.http.get<ValidationResult>(
      `${this.base}/${id}/validate`
    );
  }
}
