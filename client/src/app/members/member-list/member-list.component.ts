import { Component, inject, NgModule, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { MemberCardComponent } from "../member-card/member-card.component";
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { AccountsService } from '../../_services/accounts.service';
import { UserParams } from '../../_models/userParams';
import { FormsModule, NgForm } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';


@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent,PaginationModule,FormsModule,ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
  private accountService= inject(AccountsService);
  memberService = inject(MembersService);
  pageNumber = 1;
  pageSize = 5;
  userParams = new UserParams(this.accountService.currentUser());
  genderList = [{value:'male' , display:'Males'} , {value:'female' , display:'FeMales'}]


  ngOnInit(): void {
    if (!this.memberService.paginatedResult()) this.loadMember();
  }

  loadMember() {
    this.memberService.getmembers(this.userParams);
  }

  resetFilters() {
    this.userParams = new UserParams(this.accountService.currentUser());
    this.loadMember();
  }

  pageChanged (event:any) {
    if (this.userParams.pageNumber != event.page) {
      this.userParams.pageNumber = event.page;
      this.loadMember();

    }
  }

}
