import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { HttpEventType } from '@angular/common/http';

import { CurrentAccountComponent } from './current-account';
import { AccountService } from '../../services/account.service';
import { PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';
import { DocumentProcessingHubService } from '../../services/document-processing-hub.service';
import { MovementDto } from '../../models/movement';

describe('CurrentAccountComponent', () => {
  let component: CurrentAccountComponent;
  let fixture: ComponentFixture<CurrentAccountComponent>;
  let accountServiceSpy: { getMovements: jest.Mock; updateMovementNote: jest.Mock };
  let pdfUploadServiceSpy: { uploadPdf: jest.Mock; getHistory: jest.Mock };
  let documentProcessedSubject: Subject<UploadHistoryItem>;
  let documentHubSpy: { connect: jest.Mock; disconnect: jest.Mock; onDocumentProcessed: jest.Mock };

  const mockMovements: MovementDto[] = [
    {
      id: 'm1',
      amount: 1000,
      type: 'C',
      balance: 1000,
      description: 'Salário',
      userNote: null,
      transactionDate: '2026-06-01T10:00:00Z',
      valueDate: '2026-06-01T10:00:00Z',
      createdAt: '2026-06-01T10:00:00Z'
    },
    {
      id: 'm2',
      amount: 150,
      type: 'D',
      balance: 850,
      description: 'Renda',
      userNote: 'Casa',
      transactionDate: '2026-06-05T10:00:00Z',
      valueDate: '2026-06-05T10:00:00Z',
      createdAt: '2026-06-05T10:00:00Z'
    }
  ];

  beforeEach(async () => {
    accountServiceSpy = {
      getMovements: jest.fn().mockReturnValue(of(mockMovements)),
      updateMovementNote: jest.fn()
    };
    pdfUploadServiceSpy = {
      uploadPdf: jest.fn(),
      getHistory: jest.fn()
    };
    documentProcessedSubject = new Subject<UploadHistoryItem>();
    documentHubSpy = {
      connect: jest.fn(),
      disconnect: jest.fn(),
      onDocumentProcessed: jest.fn().mockReturnValue(documentProcessedSubject.asObservable())
    };

    await TestBed.configureTestingModule({
      imports: [CurrentAccountComponent],
      providers: [
        { provide: AccountService, useValue: accountServiceSpy },
        { provide: PdfUploadService, useValue: pdfUploadServiceSpy },
        { provide: DocumentProcessingHubService, useValue: documentHubSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CurrentAccountComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load movements and derive the current balance from the latest entry', () => {
    fixture.detectChanges();
    expect(component.currentBalance).toBe(850);
    expect(component.totalCredits).toBe(1000);
    expect(component.totalDebits).toBe(150);
    expect(component.filteredMovements.length).toBe(2);
  });

  it('should surface an error message when loading movements fails', () => {
    accountServiceSpy.getMovements.mockReturnValue(throwError(() => new Error('network error')));
    fixture.detectChanges();
    expect(component.errorMessage).toBeTruthy();
    expect(component.isLoading).toBe(false);
  });

  it('should filter movements by type', () => {
    fixture.detectChanges();
    component.selectedTypeFilter = 'D';
    component.applyFilters();
    expect(component.filteredMovements.length).toBe(1);
    expect(component.filteredMovements[0].type).toBe('D');
  });

  it('should save an edited note through the account service', () => {
    accountServiceSpy.updateMovementNote.mockReturnValue(of(undefined));
    fixture.detectChanges();

    const movement = component.allMovements[0];
    component.startEditingNote(movement);
    component.noteDraft = 'Nota de teste';
    component.saveNote(movement);

    expect(accountServiceSpy.updateMovementNote).toHaveBeenCalledWith('m1', 'Nota de teste');
    expect(movement.userNote).toBe('Nota de teste');
    expect(component.editingMovementId).toBeNull();
  });

  it('should paginate movements according to the page size', () => {
    const manyMovements: MovementDto[] = Array.from({ length: 25 }, (_, i) => ({
      id: `m${i}`,
      amount: 10,
      type: 'C',
      balance: 10 * (i + 1),
      description: `Movimento ${i}`,
      userNote: null,
      transactionDate: new Date(2026, 0, i + 1).toISOString(),
      valueDate: new Date(2026, 0, i + 1).toISOString(),
      createdAt: new Date(2026, 0, i + 1).toISOString()
    }));
    accountServiceSpy.getMovements.mockReturnValue(of(manyMovements));

    fixture.detectChanges();

    expect(component.pagedMovements.length).toBe(component.pageSize);
    expect(component.totalPages).toBe(3);

    component.goToPage(2);
    expect(component.currentPage).toBe(2);
    expect(component.pagedMovements.length).toBe(component.pageSize);
  });

  it('should track upload progress and move to processing once the upload completes', () => {
    const uploadSubject = new Subject<any>();
    pdfUploadServiceSpy.uploadPdf.mockReturnValue(uploadSubject.asObservable());
    fixture.detectChanges();

    const file = new File(['dummy'], 'statement.pdf', { type: 'application/pdf' });
    const inputEvent = { target: { files: [file], value: '' } } as unknown as Event;
    component.onStatementFileSelected(inputEvent);

    expect(pdfUploadServiceSpy.uploadPdf).toHaveBeenCalledWith(file, 'ActivoBankStatement');
    expect(component.uploadState).toBe('uploading');

    uploadSubject.next({ type: HttpEventType.UploadProgress, loaded: 50, total: 100 });
    expect(component.uploadProgress).toBe(50);

    uploadSubject.next({ type: HttpEventType.Response, body: { documentId: 'doc-1' } });
    expect(component.uploadState).toBe('processing');

    accountServiceSpy.getMovements.mockClear();
    documentProcessedSubject.next({
      id: 'doc-1',
      fileName: 'statement.pdf',
      documentType: 'ActivoBankStatement',
      fileSizeBytes: 100,
      uploadedAt: '2026-06-01T10:00:00Z',
      processedAt: '2026-06-01T10:01:00Z',
      success: true,
      errorMessage: null
    });

    expect(component.uploadState).toBe('success');
    expect(accountServiceSpy.getMovements).toHaveBeenCalled();
  });

  it('should disconnect from the document processing hub on destroy', () => {
    fixture.detectChanges();
    component.ngOnDestroy();
    expect(documentHubSpy.disconnect).toHaveBeenCalled();
  });
});
