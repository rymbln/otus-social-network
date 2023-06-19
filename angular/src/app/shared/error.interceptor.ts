import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { AuthService } from "../auth.service";
import { Router } from "@angular/router";
import { Observable, catchError, throwError } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(
      private auth: AuthService,
      private router: Router,
      ) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any | null>> {
        return next.handle(request).pipe(
          catchError(err => {
            console.log('err intercept');
            console.log(err);
            if (err.status == 401) {
              console.log('err intercept 401');
                // auto logout if 401 or 403 response returned from api
                // this.authenticationService.logout();
                return next.handle(request);
            } else if (err.status == 403) {
              console.log('err intercept 403');
              this.router.navigate(['/forbidden']);
              return next.handle(request);
            }
            // console.log('err intercep:');
            console.error(err);
            let error: string = (err && err.error && err.error.message) || err.statusText;
            console.error(error);
            if (error.startsWith("IDX10223")) {
              this.auth.logout();
            }
            return throwError(error);
        }));
    }
}
