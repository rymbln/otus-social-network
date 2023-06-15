import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { MenubarModule } from 'primeng/menubar';
import { CheckboxModule } from 'primeng/checkbox';

import { LoginComponent } from './login/login.component';
import { EmptyComponent } from './empty/empty.component';
import { ButtonModule } from 'primeng/button';
import { RegisterComponent } from './register/register.component';
import { AuthService } from './auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { JwtInterceptor } from './shared/jwt.interceptor';
import { AuthInterceptor } from './shared/auth.interceptor';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PostsComponent } from './posts/posts.component';
import { ProfileComponent } from './profile/profile.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    EmptyComponent,
    RegisterComponent,
    PostsComponent,
    ProfileComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MenubarModule,
    CheckboxModule,
    ButtonModule,
    InputTextModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: 'LOCALSTORAGE', useValue: window.localStorage },
    AuthService],
  bootstrap: [AppComponent]
})
export class AppModule { }
