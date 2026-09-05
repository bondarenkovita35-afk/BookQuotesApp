import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { Register } from './register';

describe('Register', () => {
  let fixture: ComponentFixture<Register>;
  let component: Register;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('is invalid when the password is shorter than 8 characters', () => {
    component.form.setValue({ email: 'a@b.com', password: 'short', confirmPassword: 'short' });

    expect(component.form.invalid).toBeTrue();
  });

  it('is invalid when the passwords do not match', () => {
    component.form.setValue({ email: 'a@b.com', password: 'Password123', confirmPassword: 'Password124' });

    expect(component.form.errors?.['passwordMismatch']).toBeTrue();
  });

  it('is valid when both passwords match and meet the length requirement', () => {
    component.form.setValue({ email: 'a@b.com', password: 'Password123', confirmPassword: 'Password123' });

    expect(component.form.valid).toBeTrue();
  });

  it('navigates to /books after a successful registration', () => {
    authServiceSpy.register.and.returnValue(
      of({ token: 't', expiresAtUtc: new Date().toISOString(), email: 'a@b.com' }),
    );
    spyOn(router, 'navigateByUrl');
    component.form.setValue({ email: 'a@b.com', password: 'Password123', confirmPassword: 'Password123' });

    component.submit();

    expect(authServiceSpy.register).toHaveBeenCalledWith({ email: 'a@b.com', password: 'Password123' });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/books');
  });

  it('shows a conflict error message when the email is already registered', () => {
    authServiceSpy.register.and.returnValue(
      throwError(() => ({ error: { message: 'E-postadressen används redan.' } })),
    );
    component.form.setValue({ email: 'a@b.com', password: 'Password123', confirmPassword: 'Password123' });

    component.submit();

    expect(component.errorMessage()).toBe('E-postadressen används redan.');
  });
});
