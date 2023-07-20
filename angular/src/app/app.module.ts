import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app.routing';
import { AppComponent } from './app.component';

import { MenubarModule } from 'primeng/menubar';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';
import { DialogModule } from 'primeng/dialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageModule } from 'primeng/message';
import { MessagesModule } from 'primeng/messages';

import { LoginComponent } from './login/login.component';
import { EmptyComponent } from './empty/empty.component';
import { ButtonModule } from 'primeng/button';
import { RegisterComponent } from './register/register.component';
import { AuthService } from './auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { JwtInterceptor } from './shared/jwt.interceptor';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PostsComponent } from './posts/posts.component';
import { ProfileComponent } from './profile/profile.component';
import { UserService } from './user.service';
import { PostService } from './post.service';
import { AuthGuard } from './shared/auth.guard';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { ErrorInterceptor } from './shared/error.interceptor';
import { FriendService } from './friend.service';
import { FriendsComponent } from './friends/friends.component';
import { FeedComponent } from './feed/feed.component';
import { NgChartsModule } from 'ng2-charts';
import { PostHotificationService as PostNotificationService } from './post-hotification.service';
import { ChatsComponent } from './chats/chats.component';
import { DataViewModule, DataViewLayoutOptions } from 'primeng/dataview';
import { DropdownModule } from 'primeng/dropdown';
import { ScrollPanelModule } from 'primeng/scrollpanel';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    EmptyComponent,
    RegisterComponent,
    PostsComponent,
    ProfileComponent,
    FriendsComponent,
    FeedComponent,
    ChatsComponent,
    ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MenubarModule,
    CheckboxModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    ToolbarModule,
    DialogModule,
    InputTextareaModule,
    ConfirmDialogModule,
    NgChartsModule,
    ToastModule,
    MessagesModule,
    MessageModule,
    DataViewModule,
    DropdownModule,
    ScrollPanelModule
  ],
  providers: [
    AuthGuard,
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: 'LOCALSTORAGE', useValue: window.localStorage },
    AuthService, UserService, PostService, FriendService, PostNotificationService,
    ConfirmationService,
  MessageService],
  bootstrap: [AppComponent]
})
export class AppModule { }
