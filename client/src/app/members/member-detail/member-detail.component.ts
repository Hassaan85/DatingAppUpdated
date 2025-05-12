import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessagesService } from '../../_services/messages.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountsService } from '../../_services/accounts.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit  , OnDestroy{
  @ViewChild('memberTabs' , {static:true}) memberTabs?:TabsetComponent;
   presenceService = inject(PresenceService);
  private messageService = inject(MessagesService);
  private accountService = inject(AccountsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  member: Member = {} as Member;
  images: GalleryItem[]=[];
  activeTab?:TabDirective;
  // messages:Message[] = []

  ngOnInit(): void {
    this.route.data.subscribe({
      next : data => {
        this.member = data['member'];
       this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({src:p.url, thumb: p.url}))
        })
      }
    })

    this.route.paramMap.subscribe({
      next: _ => this.onRouterParamsChanged()
    })

    this.route.queryParams.subscribe({
      next : params  => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
    
  }

  onRouterParamsChanged() {
    const user  = this.accountService.currentUser();
    if (!user) return;
    if (this.messageService.hubConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages')
    {
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username );
      })
    }
  }

  onTabActivated (data : TabDirective) {
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo:this.route,
      queryParams : {tab : this.activeTab.heading},
      queryParamsHandling : 'merge'
    })
    if (this.activeTab.heading === 'Messages'  && this.member) {
      // this.messageService.getMessageThread(this.member.username).subscribe({
      //   next :
      //    messages =>   this.messages = messages
         
      // })
      const user = this.accountService.currentUser();
      if(!user) return
      this.messageService.createHubConnection(user , this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  // onUpdatedMessage(event:Message) {
  //   this.messages.push(event);
  // }

  selectTab(heading :string) {
    if (this.memberTabs) {
      const messageTab =  this.memberTabs.tabs.find(x => x.heading === heading);
      if(messageTab) messageTab.active=true;
    }
  }

//   loadMember() {
//     const username  = this.route.snapshot.paramMap.get('username');
//     if (!username) return;
//     this.memberService.getmember(username).subscribe({
//       next : member => { this.member = member;
//       member.photos.map(p => {
//         this.images.push(new ImageItem({src:p.url, thumb: p.url}))
//       })
//     }
//     })
//   }

   ngOnDestroy(): void {
     this.messageService.stopHubConnection();
   }
 }
