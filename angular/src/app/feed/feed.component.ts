import { Component } from '@angular/core';
import { BehaviorSubject, Observable, shareReplay, switchMap, tap } from 'rxjs';
import { PostDto, PostView } from '../model/post.model';
import { PostService } from '../post.service';

@Component({
  selector: 'app-feed',
  templateUrl: './feed.component.html',
  styleUrls: ['./feed.component.scss']
})
export class FeedComponent {
  private _refresh = new BehaviorSubject<boolean>(false);

  posts$: Observable<PostDto[]> = this._refresh.asObservable().pipe(
    switchMap(e => this.srv.getFeed()),
    tap(data => console.log(data)),
    shareReplay()
  );

  constructor(private srv: PostService) {

  }

  onRefresh() {
    this._refresh.next(true);
  }
}
