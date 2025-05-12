import { AfterViewChecked, Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { MessagesService } from '../../_services/messages.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule , FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm? : NgForm;
  @ViewChild("scrollMe") scrollContainer? : any ;
  username  = input.required<string>();
  // messages = input.required<Message[]>();
  messageService = inject(MessagesService);
  messageContent = '';
  // updateMessages = output<Message>();

ngAfterViewChecked(): void {
  this.scrollToBottom();
}

private scrollToBottom () {
  if (this.scrollContainer)
  {
    this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
  }
}
  sendMessage() {
    this.messageService.sendMessage(this.username() , this.messageContent).then(()=> {
      this.messageForm?.reset();
      this.scrollToBottom();
    })
  }



  // loadMessages() {
  //   this.messageService.getMessageThread(this.username()).subscribe({
  //     next :
  //      messages =>   this.messages = messages
       
  //   })
  // }
}
