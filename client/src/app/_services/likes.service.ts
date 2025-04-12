import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { PaginationedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {

  baseurl = environment.apiUrl;
  private http = inject(HttpClient);
  likeIds = signal<number[]>([]);
  paginatedResult = signal<PaginationedResult<Member[]>| null>(null);


  toggleLike(targetId : number) {
    return this.http.post(`${this.baseurl}likes/${targetId}`, {})
  }

  getLikes(predicate : string , pageNumber:number , pageSize:number) {
  let params = setPaginationHeaders(pageNumber , pageSize);

  params = params.append('predicate' , predicate);

     return this.http.get<Member[]>(`${this.baseurl}likes?predicate=${predicate}` ,{observe : 'response' , params}).subscribe({
      next : response => setPaginatedResponse(response ,this.paginatedResult)
     });
  }

  getLikeIds() {
    return this.http.get<number[]>(`${this.baseurl}likes/list`).subscribe({
      next : ids => this.likeIds.set(ids)
    })
  }
  
}
