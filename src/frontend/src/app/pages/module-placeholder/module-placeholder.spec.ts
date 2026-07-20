import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';

import { ModulePlaceholderComponent } from './module-placeholder';

describe('ModulePlaceholderComponent', () => {
  let component: ModulePlaceholderComponent;
  let fixture: ComponentFixture<ModulePlaceholderComponent>;

  // Build a minimal ActivatedRoute stub exposing the static route data used by the component
  const activatedRouteStub = {
    snapshot: { data: { moduleName: 'Investments' } }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModulePlaceholderComponent],
      providers: [{ provide: ActivatedRoute, useValue: activatedRouteStub }],
    }).compileComponents();

    fixture = TestBed.createComponent(ModulePlaceholderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should resolve the module name from the active route data', () => {
    expect(component.moduleName).toBe('Investments');
  });

  it('should render the "Under Construction" message with the module name', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Under Construction - Investments');
  });
});
