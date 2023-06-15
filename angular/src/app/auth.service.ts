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
  currentUser: any;

  private _isAuth = new BehaviorSubject<boolean>(false);
  isAuth$: Observable<boolean> = this._isAuth.asObservable().pipe(shareReplay());
  get isAuth(): boolean {
    return this._isAuth.value;
  }

  jwt: string = '';
  constructor(    @Inject('LOCALSTORAGE') private localStorage: Storage,
  private _http: HttpClient,
  private _router: Router) { }

  login(obj: LoginReq): Promise<boolean> {
    const res = firstValueFrom(this._http.post<any>(`${environment.api}/login`,obj)
    .pipe(
      tap(data => console.log(data)),
      map(data => {
        this.jwt = data;
        this._isAuth.next(true);
        this.localStorage.setItem('AUTH_KEY', data || '');
        const token = this.parseJwt(data.token);
        console.log(token)
        return true; }),
      catchError(err => {
        this._isAuth.next(false);
        console.log(err);
        return of(false);
      })
    ));
    return res;

      // return this._http.post<any>(`${environment.api}/login`,
      //   obj,
      //   )
      //   .pipe(
      //     tap(data => console.log(data)),
          // map(res => res.data as AuthenticationResponse),
          // map(res => {
          //   const u = new User(res);
          //   this._user.next(u);
          //   this.localStorage.setItem(AUTH_KEY, res.jwtToken || '');
          //   this.localStorage.setItem(AUTH_REFRESH_KEY, res.refreshToken || '');
              // TODO: Uncomment
            // this.stopRefreshTokenTimer();
            // this.startRefreshTokenTimer(res.jwtToken);
            // return u;
          // }));
    }

  logout() {}

  private parseJwt(token: string) {
    console.log(token);
    var base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(require('buffer').Buffer.from(base64, 'base64').toString().split('').map(function (c:any) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
  };

  // private isJwtExpired(token: string | undefined): boolean {
  //   if (token && this.parseJwt(token)) {
  //     const expiry = this.parseJwt(token).exp;
  //     const now = new Date();
  //     return now.getTime() > expiry * 1000;
  //   }
  //   return false;
  // }
}
