import { Directive, OnInit, Input, ViewContainerRef, TemplateRef } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false;


  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authService: AuthService
  ) { }
  ngOnInit(){
    
    const userRoles = this.authService.decodedToken.role as Array<string>;

    //If no roles clear the viewContainer
    if(!userRoles)
    {
      this.viewContainerRef.clear();
    }

    //If user has role then render the element
    if(this.authService.roleMatch(this.appHasRole))
    {
      if(!this.isVisible)
      {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }else{
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }

  }
}
