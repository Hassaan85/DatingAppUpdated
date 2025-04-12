import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginationedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountsService } from './accounts.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
userParams() {
throw new Error('Method not implemented.');
}
  private http = inject(HttpClient);
  private accountService = inject(AccountsService);
  baseUrl = environment.apiUrl;
 // members = signal<Member[]>([]);
  paginatedResult = signal<PaginationedResult<Member[]>| null>(null);
  memberCache= new Map();
  user = this.accountService.currentUser();
  userParam = signal<UserParams> (new UserParams(this.user));



  getmembers(UserParams : UserParams) {
    const response = this.memberCache.get(Object.values(UserParams).join('-'));

    if (response) return setPaginatedResponse(response , this.paginatedResult);

  let params = setPaginationHeaders(UserParams.pageNumber , UserParams.pageSize)

  params = params.append('minAge' , UserParams.minAge)
  params = params.append('maxAge' , UserParams.maxAge)
  params = params.append('gender' , UserParams.gender)
  params = params.append('orderBy' , UserParams.orderBy)
 
 
    return this.http.get<Member[]>(this.baseUrl + 'users',{observe:'response' , params}).subscribe({
      next: response => { 
        setPaginatedResponse(response , this.paginatedResult);
        this.memberCache.set(Object.values(UserParams).join('-'),response)
    }
    })
  }

  // private setPaginationHeaders (pageNumber : number , pageSize : number) {
  //   let params = new HttpParams();
  //   if (pageNumber && pageSize) {
  //     params = params.append('pageNumber' , pageNumber);
  //     params = params.append('pageSize' , pageSize);
  //   }

  //   return params;
    
  // }

  // private setPaginatedResponse(response  : HttpResponse<Member[]>) {
  //   this.paginatedResult.set ({
  //     items: response.body as Member[],
  //     pagination:JSON.parse(response.headers.get('Pagination')!)
  // })

  // }

  getmember(username:string){
    const member : Member = [...this.memberCache.values()]
    .reduce((arr, elem) => arr.concat(elem.body),[])
    .find((m : Member) => m.username === username);

    if (member) return of(member);

    // const member = this.members().find(x => x.username === username);
    // if (member != undefined) return of(member);
      
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updatemember(member : Member){
    return this.http.put(this.baseUrl + 'users' , member).pipe(
      // tap(()=>{
      //     this.members.update(members => members.map( m => m.username === member.username? member : m))
      // })
    )
  }

  setMainPhoto (photo:Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id,{}).pipe(
      // tap(()=>{
      //  this.members.update(members =>  members.map(m => {
      //     if(
      //       m.photos.includes(photo)) {
      //         m.photoUrl = photo.url
      //       }
      //       return m;
      //  }))
      // })
    )

  }

  deletePhoto(photo  : Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      // tap(()=>{
      //   this.members.update(members => members.map(m =>{

      //     if (m.photos.includes(photo)) {
      //       m.photos =m.photos.filter(x=> x.id !== photo.id)
      //     }
      //    return m
      //   }))

      // })
    )

  }
}
