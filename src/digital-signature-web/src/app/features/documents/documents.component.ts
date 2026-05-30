import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatToolbarModule } from '@angular/material/toolbar';
import { DocumentService, DocumentSummary, ValidationResult } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatChipsModule, MatToolbarModule
  ],
  template: `
    <mat-toolbar color="primary">
      <span>Digital Signature Platform</span>
      <span class="spacer"></span>
      <button mat-icon-button (click)="logout()" title="Sair">
        <mat-icon>logout</mat-icon>
      </button>
    </mat-toolbar>

    <div class="container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Meus Documentos</mat-card-title>
          <mat-card-subtitle>Gerencie e assine seus documentos digitalmente</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <!-- Upload -->
          <div class="upload-section">
            <button mat-raised-button color="accent" (click)="fileInput.click()" [disabled]="uploading()">
              <mat-icon>upload_file</mat-icon>
              @if (uploading()) { Enviando... } @else { Enviar Documento }
            </button>
            <input #fileInput type="file" accept=".pdf,.doc,.docx,.txt" hidden
                   (change)="onFileSelected($event)" />
            <small>PDF, DOC, DOCX ou TXT — máximo 10 MB</small>
          </div>

          <!-- Loading -->
          @if (loading()) {
            <div class="center"><mat-spinner></mat-spinner></div>
          }

          <!-- Table -->
          @if (!loading() && documents().length > 0) {
            <table mat-table [dataSource]="documents()" class="full-width">
              <ng-container matColumnDef="fileName">
                <th mat-header-cell *matHeaderCellDef>Arquivo</th>
                <td mat-cell *matCellDef="let d">{{ d.fileName }}</td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let d">
                  <mat-chip [color]="d.status === 'Signed' ? 'primary' : 'accent'" highlighted>
                    {{ d.status === 'Signed' ? 'Assinado' : 'Enviado' }}
                  </mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="uploadedAt">
                <th mat-header-cell *matHeaderCellDef>Data</th>
                <td mat-cell *matCellDef="let d">{{ d.uploadedAt | date:'dd/MM/yyyy HH:mm' }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Ações</th>
                <td mat-cell *matCellDef="let d">
                  <button mat-icon-button color="primary"
                          (click)="sign(d.id)" [disabled]="d.status === 'Signed'"
                          title="Assinar">
                    <mat-icon>draw</mat-icon>
                  </button>
                  <button mat-icon-button color="accent"
                          (click)="validate(d.id)" title="Validar Assinatura">
                    <mat-icon>verified</mat-icon>
                  </button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="columns"></tr>
              <tr mat-row *matRowDef="let row; columns: columns;"></tr>
            </table>
          }

          @if (!loading() && documents().length === 0) {
            <p class="empty">Nenhum documento encontrado. Envie seu primeiro documento!</p>
          }

          <!-- Validation result -->
          @if (validationResult()) {
            <mat-card [class]="validationResult()!.isValid ? 'valid-card' : 'invalid-card'">
              <mat-card-content>
                <p>
                  <mat-icon>{{ validationResult()!.isValid ? 'verified' : 'cancel' }}</mat-icon>
                  <strong>{{ validationResult()!.isValid ? 'Assinatura Válida' : 'Assinatura Inválida' }}</strong>
                </p>
                @if (validationResult()!.algorithm) {
                  <p>Algoritmo: {{ validationResult()!.algorithm }}</p>
                }
                @if (validationResult()!.signedAt) {
                  <p>Assinado em: {{ validationResult()!.signedAt | date:'dd/MM/yyyy HH:mm' }}</p>
                }
                @if (validationResult()!.failureReason) {
                  <p class="error">{{ validationResult()!.failureReason }}</p>
                }
              </mat-card-content>
            </mat-card>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .spacer { flex:1; }
    .container { padding:24px; max-width:1000px; margin:0 auto; }
    .upload-section { display:flex; align-items:center; gap:16px; margin-bottom:24px; }
    .center { display:flex; justify-content:center; padding:32px; }
    .full-width { width:100%; }
    .empty { text-align:center; color:#666; padding:32px; }
    .valid-card { background:#e8f5e9; margin-top:16px; }
    .invalid-card { background:#ffebee; margin-top:16px; }
    mat-icon { vertical-align:middle; margin-right:4px; }
    .error { color:#c62828; }
  `]
})
export class DocumentsComponent implements OnInit {
  readonly columns = ['fileName', 'status', 'uploadedAt', 'actions'];
  documents = signal<DocumentSummary[]>([]);
  loading = signal(true);
  uploading = signal(false);
  validationResult = signal<ValidationResult | null>(null);

  constructor(
    private docService: DocumentService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() { this.loadDocuments(); }

  loadDocuments() {
    this.loading.set(true);
    this.docService.list().subscribe({
      next: docs => { this.documents.set(docs); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.uploading.set(true);
    this.docService.upload(file).subscribe({
      next: () => {
        this.snackBar.open('Documento enviado!', 'OK', { duration: 3000 });
        this.uploading.set(false);
        this.loadDocuments();
      },
      error: (err) => {
        this.snackBar.open(err.error?.error ?? 'Erro ao enviar.', 'OK', { duration: 4000 });
        this.uploading.set(false);
      }
    });
  }

  sign(id: string) {
    this.docService.sign(id).subscribe({
      next: () => {
        this.snackBar.open('Documento assinado!', 'OK', { duration: 3000 });
        this.loadDocuments();
      },
      error: (err) => {
        this.snackBar.open(err.error?.error ?? 'Erro ao assinar.', 'OK', { duration: 4000 });
      }
    });
  }

  validate(id: string) {
    this.docService.validate(id).subscribe({
      next: (result) => { this.validationResult.set(result); },
      error: () => { this.snackBar.open('Erro ao validar.', 'OK', { duration: 4000 }); }
    });
  }

  logout() { this.authService.logout(); }
}
