import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { HttpEventType } from '@angular/common/http';

import { CurrentAccountComponent } from './current-account';
import { AccountService } from '../../services/account.service';
import { PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';
import { DocumentProcessingHubService } from '../../services/document-processing-hub.service';
import { MovementDto, MovementTotals, PagedMovementsResponse } from '../../models/movement';

describe('CurrentAccountComponent', () => {
  let component: CurrentAccountComponent;
  let fixture: ComponentFixture<CurrentAccountComponent>;
  let accountServiceSpy: { getMovements: jest.Mock; getMovementTotals: jest.Mock; updateMovementNote: jest.Mock };
  let pdfUploadServiceSpy: { uploadPdf: jest.Mock; getHistory: jest.Mock };
  let documentProcessedSubject: Subject<UploadHistoryItem>;
  let documentHubSpy: { connect: jest.Mock; disconnect: jest.Mock; onDocumentProcessed: jest.Mock };

  const mockMovements: MovementDto[] = [
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
    },
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
    }
  ];

  const mockTotals: MovementTotals = {
    totalCredits: 1000,
    totalDebits: 150,
    currentBalance: 850
  };

  function pagedResponse(items: MovementDto[], page = 1, pageSize = 10): PagedMovementsResponse {
    return {
      items,
      page,
      pageSize,
      totalCount: items.length,
      totalPages: Math.max(1, Math.ceil(items.length / pageSize))
    };
  }

  beforeEach(async () => {
    accountServiceSpy = {
      getMovements: jest.fn().mockReturnValue(of(pagedResponse(mockMovements))),
      getMovementTotals: jest.fn().mockReturnValue(of(mockTotals)),
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

  it('should load the current page of movements and the summary totals', () => {
    fixture.detectChanges();
    expect(component.currentBalance).toBe(850);
    expect(component.totalCredits).toBe(1000);
    expect(component.totalDebits).toBe(150);
    expect(component.pagedMovements.length).toBe(2);
  });

  it('should surface an error message when loading movements fails', () => {
    accountServiceSpy.getMovements.mockReturnValue(throwError(() => new Error('network error')));
    fixture.detectChanges();
    expect(component.errorMessage).toBeTruthy();
    expect(component.isLoading).toBe(false);
  });

  it('should send the active date filter to the backend and reset to page 1', () => {
    fixture.detectChanges();
    accountServiceSpy.getMovements.mockClear();
    accountServiceSpy.getMovementTotals.mockClear();

    component.dateFilterMode = 'valueDate';
    component.dateFrom = '2026-06-01';
    component.dateTo = '2026-06-30';
    component.currentPage = 3;
    component.applyDateFilter();

    expect(component.currentPage).toBe(1);
    expect(accountServiceSpy.getMovements).toHaveBeenCalledWith(
      expect.objectContaining({ page: 1, valueDateFrom: '2026-06-01', valueDateTo: '2026-06-30' })
    );
    expect(accountServiceSpy.getMovementTotals).toHaveBeenCalled();
  });

  it('should clear the active date filter and reload', () => {
    fixture.detectChanges();
    component.dateFrom = '2026-06-01';
    component.dateTo = '2026-06-30';

    component.clearDateFilter();

    expect(component.dateFrom).toBe('');
    expect(component.dateTo).toBe('');
  });

  it('should save an edited note through the account service and sync from its response', () => {
    const updatedMovement: MovementDto = { ...mockMovements[1], userNote: 'Nota de teste' };
    accountServiceSpy.updateMovementNote.mockReturnValue(of(updatedMovement));
    fixture.detectChanges();

    const movement = component.pagedMovements.find((m) => m.id === 'm1')!;
    component.startEditingNote(movement);
    component.noteDraft = 'Nota de teste';
    component.saveNote(movement);

    expect(accountServiceSpy.updateMovementNote).toHaveBeenCalledWith('m1', 'Nota de teste');
    expect(movement.userNote).toBe('Nota de teste');
    expect(component.editingMovementId).toBeNull();
  });

  it('should surface an error message when saving a note fails', () => {
    accountServiceSpy.updateMovementNote.mockReturnValue(throwError(() => new Error('network error')));
    fixture.detectChanges();

    const movement = component.pagedMovements[0];
    component.startEditingNote(movement);
    component.noteDraft = 'Nota de teste';
    component.saveNote(movement);

    expect(component.errorMessage).toBeTruthy();
    expect(component.isSavingNote).toBe(false);
  });

  it('should request the next page from the backend instead of paginating locally', () => {
    accountServiceSpy.getMovements.mockReturnValue(of(pagedResponse(mockMovements, 1, 10)));
    fixture.detectChanges();

    accountServiceSpy.getMovements.mockClear();
    accountServiceSpy.getMovements.mockReturnValue(of(pagedResponse(mockMovements, 2, 10)));
    component.totalPages = 3;

    component.goToPage(2);

    expect(component.currentPage).toBe(2);
    expect(accountServiceSpy.getMovements).toHaveBeenCalledWith(expect.objectContaining({ page: 2 }));
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
