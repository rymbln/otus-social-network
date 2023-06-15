import { Router } from "@angular/router";
import { AuthService } from "../auth.service";
import { Injectable } from "@angular/core";
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Observable, tap } from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private auth: AuthService,
        private router: Router,
        ) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        const user = this.auth.currentUser;

        if (user && user.jwtToken) {

            const cloned = req.clone({
                headers: req.headers.set('Authorization',
                    'Bearer ' + user.jwtToken)
            });

            return next.handle(cloned).pipe(tap(() => { }, (err: any) => {
                if (err instanceof HttpErrorResponse) {
                    if (err.status === 401) {
                        this.router.navigate(['/auth/signin']);
                    }
                }
            }));

        } else {
            return next.handle(req);
        }
    }
}
