import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from "rxjs";
import { AuthService } from "../auth.service";
import { environment } from "src/environments/environment";
import { Router } from "@angular/router";

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private auth: AuthService, private router: Router) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // add auth header with jwt if user is logged in and request is to the api url
    const hasAuthData = this.auth.isAuth;
    const jwt = this.auth.jwt;
    const isApiUrl = request.url.startsWith(environment.api);
    if (hasAuthData && isApiUrl) {
      // console.log('jwt intercept add: ' + jwt);
      request = this.addTokenHeader(request, jwt);
    }
    return next.handle(request);
  }

  private addTokenHeader(request: HttpRequest<any>, token: string) {
    return request = request.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }
}
