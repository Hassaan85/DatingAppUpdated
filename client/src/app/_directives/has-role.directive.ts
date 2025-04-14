import { Directive, inject, Input, input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountsService } from '../_services/accounts.service';

@Directive({
  selector: '[appHasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {

  @Input() appHasRole: string[] = [];
  private accountService = inject(AccountsService);
  private viewContaineRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef);

  ngOnInit(): void {
    if (this.accountService.roles().some((r:string) => this.appHasRole.includes(r))){
      this.viewContaineRef.createEmbeddedView(this.templateRef)
    } else {
      this.viewContaineRef.clear();
    }
  }

}
