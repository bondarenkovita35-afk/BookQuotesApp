import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { Login } from './login';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('starts with an invalid, empty form', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('does not call the API when the form is invalid', () => {
    component.submit();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('navigates to /books after a successful login', () => {
    authServiceSpy.login.and.returnValue(
      of({ token: 't', expiresAtUtc: new Date().toISOString(), email: 'a@b.com' }),
    );
    spyOn(router, 'navigateByUrl');
    component.form.setValue({ email: 'a@b.com', password: 'Password123' });

    component.submit();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/books');
  });

  it('shows the server error message when login fails', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ error: { message: 'Fel e-postadress eller lösenord.' } })),
    );
    component.form.setValue({ email: 'a@b.com', password: 'wrong' });

    component.submit();

    expect(component.errorMessage()).toBe('Fel e-postadress eller lösenord.');
    expect(component.isSubmitting()).toBeFalse();
  });

  it('ignores a second submit while a request is in flight', () => {
    authServiceSpy.login.and.returnValue(
      of({ token: 't', expiresAtUtc: new Date().toISOString(), email: 'a@b.com' }),
    );
    component.form.setValue({ email: 'a@b.com', password: 'Password123' });

    component.isSubmitting.set(true);
    component.submit();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });
});
