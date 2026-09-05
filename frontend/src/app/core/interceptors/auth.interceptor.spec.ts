import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getToken']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('adds an Authorization header when a token exists', () => {
    authServiceSpy.getToken.and.returnValue('my-token');

    httpClient.get('/api/books').subscribe();

    const req = httpMock.expectOne('/api/books');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-token');
    req.flush([]);
  });

  it('does not add an Authorization header when there is no token', () => {
    authServiceSpy.getToken.and.returnValue(null);

    httpClient.get('/api/books').subscribe();

    const req = httpMock.expectOne('/api/books');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush([]);
  });
});
