import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from "rxjs";
import { AuthService } from "../auth.service";
import { environment } from "src/environments/environment";

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private authenticationService: AuthService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // add auth header with jwt if user is logged in and request is to the api url
    const hasAuthData = this.authenticationService.isAuth;
    const jwt = this.authenticationService.jwt;
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
