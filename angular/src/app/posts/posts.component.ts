import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { PostService } from '../post.service';
import { PostDto, PostHubModel } from '../model/post.model';
import { BehaviorSubject, Observable, combineLatest, firstValueFrom, map, shareReplay, switchMap, tap } from 'rxjs';
import { CreatePostReq } from '../model/create-post.req';
import { UpdatePostReq } from '../model/update-post.req';
import { AuthService } from '../auth.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { PostHotificationService } from '../post-hotification.service';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';

@UntilDestroy()
@Component({
  selector: 'app-posts',
  templateUrl: './posts.component.html',
  styleUrls: ['./posts.component.scss']
})
export class PostsComponent implements OnInit {

  isNewPostDialog = false;
  newPostText = '';
  isAdd = false;
  postId: string = '';

  private _refresh = new BehaviorSubject<boolean>(false);

  posts$: Observable<PostDto[]> = this._refresh.asObservable().pipe(
    switchMap(e => this.srv.getPosts()),
    tap(data => console.log(data)),
    shareReplay()
  );

  constructor(
    private srv: PostService,
    private signal: PostHotificationService,
    private auth: AuthService,
    private confirm: ConfirmationService,
    private http: HttpClient,
) { }

  ngOnInit(): void {
    this._refresh.next(true);

  }
  onNew() {
    this.isAdd = true;
    this.isNewPostDialog = true;
    this.newPostText = '';


  }



  onRefresh() {
    this._refresh.next(true);
  }

  onEdit(post: PostDto) {
    this.isAdd = false;
    this.newPostText = post.text;
    this.postId = post.id;
    this.isNewPostDialog = true;
  }

  onDelete(post: PostDto) {
    this.confirm.confirm({
      message: `Are you sure you want to delete post ${ post.id }?`,
      header: `Confirm delete`,
      icon: 'pi pi-info-circle',
      accept: () => {
        this.srv.deletePost(post.id).pipe(
          untilDestroyed(this),
          tap(e => this._refresh.next(true))
        ).subscribe()
      },
    });
  }

  onSave() {
    if (this.isAdd) {
      const data = new CreatePostReq(this.newPostText);
      this.srv.createPost(data).pipe(
        untilDestroyed(this),
        tap((data: PostDto) => {
          console.log(data);
          this._refresh.next(true);
          this.isNewPostDialog = false;
          //// Send notification about new post
          // const postHub = { postId: data.id, postText: data.text, authorUserId: data.authorUserId} as PostHubModel;
          // console.log('posthub:', postHub);
          // this.signal.sendPostNotification(postHub);
        })
      ).subscribe();
    } else {
      const data = new UpdatePostReq(this.postId, this.newPostText);
      this.srv.updatePost(data).pipe(
        untilDestroyed(this),
        tap(e => {
          console.log(e);
        this._refresh.next(true);
        this.isNewPostDialog = false;
        })
      ).subscribe();
    }
  }
}
function UntilDestory(): (target: typeof PostsComponent) => void | typeof PostsComponent {
  throw new Error('Function not implemented.');
}

