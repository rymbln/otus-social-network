import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PostDto, PostView } from './model/post.model';
import { environment } from 'src/environments/environment';
import { CreatePostReq } from './model/create-post.req';
import { UpdatePostReq } from './model/update-post.req';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PostService {

  constructor( private _http: HttpClient) { }

  getPosts(): Observable<PostDto[]> {
    return this._http.get<PostDto[]>(`${environment.api}/post`);
  }
  getPost(id: string) {
    return this._http.get<PostDto>(`${environment.api}/post/${id}`);
  }
  getFeed(): Observable<PostDto[]> {
    return this._http.get<PostDto[]>(`${environment.api}/post/feed`);
  }
  createPost(obj: CreatePostReq) {
    return this._http.post<string>(`${environment.api}/post/create`, obj);
  }
  updatePost(obj: UpdatePostReq) {
    return this._http.put<string>(`${environment.api}/post/update`, obj);
  }
  deletePost(id: string) {
    return this._http.delete(`${environment.api}/post/${id}`);
  }
}
