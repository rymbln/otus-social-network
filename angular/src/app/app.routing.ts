import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmptyComponent } from './empty/empty.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ProfileComponent } from './profile/profile.component';
import { PostsComponent } from './posts/posts.component';
import { AuthGuard } from './shared/auth.guard';
import { FriendsComponent } from './friends/friends.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: ProfileComponent,
  },
  {
    path: 'posts',
    component: PostsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'friends',
    component: FriendsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
