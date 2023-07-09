import { Component } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, delay, shareReplay, switchMap, tap } from 'rxjs';
import { PostDto, PostView } from '../model/post.model';
import { PostService } from '../post.service';
import { PostHotificationService } from '../post-hotification.service';

@Component({
  selector: 'app-feed',
  templateUrl: './feed.component.html',
  styleUrls: ['./feed.component.scss']
})
export class FeedComponent {
  private _refresh = new BehaviorSubject<boolean>(false);

  posts$: Observable<PostDto[]> = combineLatest([this._refresh.asObservable(), this.notify.feedUpdated$.pipe(delay(2000))]).pipe(
    tap(data => console.log(data)),
    switchMap(e => this.srv.getFeed()),
    tap(data => console.log(data.length)),
    shareReplay()
  );

  constructor(private srv: PostService, private notify: PostHotificationService) {

  }

  onRefresh() {
    this._refresh.next(true);
  }
}
