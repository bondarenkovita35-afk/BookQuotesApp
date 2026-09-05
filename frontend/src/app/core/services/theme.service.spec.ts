import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  const THEME_KEY = 'bookquotesapp.theme';

  beforeEach(() => {
    localStorage.removeItem(THEME_KEY);
    document.documentElement.removeAttribute('data-bs-theme');
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.removeItem(THEME_KEY);
    document.documentElement.removeAttribute('data-bs-theme');
  });

  it('defaults to light when nothing is saved and the system prefers light', () => {
    spyOn(window, 'matchMedia').and.returnValue({ matches: false } as MediaQueryList);

    const service = TestBed.inject(ThemeService);

    expect(service.theme()).toBe('light');
  });

  it('uses the saved theme instead of the system preference when one exists', () => {
    localStorage.setItem(THEME_KEY, 'dark');

    const service = TestBed.inject(ThemeService);

    expect(service.theme()).toBe('dark');
  });

  it('toggle() switches between light and dark and persists the choice', () => {
    localStorage.setItem(THEME_KEY, 'light');
    const service = TestBed.inject(ThemeService);

    service.toggle();
    TestBed.flushEffects();

    expect(service.theme()).toBe('dark');
    expect(localStorage.getItem(THEME_KEY)).toBe('dark');
    expect(document.documentElement.getAttribute('data-bs-theme')).toBe('dark');
  });
});
