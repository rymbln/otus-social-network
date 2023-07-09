import { Inject, Injectable } from '@angular/core';
import { LoginReq } from './model/login.req';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { BehaviorSubject, Observable, catchError, firstValueFrom, map, of, shareReplay, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  currentUser = '';

  private readonly ClaimTypes_NameIdentifier = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
  private readonly ClaimTypes_Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

  private _isAuth = new BehaviorSubject<boolean>(false);
  isAuth$: Observable<boolean> = this._isAuth.asObservable().pipe(shareReplay());
  get isAuth(): boolean {
    return this._isAuth.value;
  }
  userId: string = '';

  readonly AUTH_KEY: string = 'auth-jwt';
  jwt: string = '';
  constructor(@Inject('LOCALSTORAGE') private localStorage: Storage,
    private _http: HttpClient,
    private _router: Router) {
      const token =  this.localStorage.getItem(this.AUTH_KEY) ?? '';
      if (token !== '') {
        this._isAuth.next(true);
        const data = this.parseJwt(token);
        console.log(data);
        this.currentUser = data[this.ClaimTypes_Name];
        this.jwt = token;
      }

  }

  login(obj: LoginReq): Promise<boolean> {
    const res = firstValueFrom(this._http.post<any>(`${environment.api}/login`, obj)
      .pipe(
        tap(data => console.log(data)),
        map(data => {
          this._isAuth.next(true);
          this.localStorage.setItem(this.AUTH_KEY, data.token);
          this.jwt = data.token;
          const token = this.parseJwt(data.token);
          this.currentUser = token[this.ClaimTypes_NameIdentifier];
          console.log(token)
          return true;
        }),
        catchError(err => {
          this._isAuth.next(false);
          console.log(err);
          return of(false);
        })
      ));
    return res;
  }

  logout() {
    this.localStorage.removeItem(this.AUTH_KEY);
    this._isAuth.next(false);
    this._router.navigate(['/login']);
  }

  private parseJwt(token: string) {
    var base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(require('buffer').Buffer.from(base64, 'base64').toString().split('').map(function (c: any) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
  };
}
