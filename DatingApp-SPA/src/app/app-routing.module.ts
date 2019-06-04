import { PreventUnsavedChanges } from './_guards/prevent-unsaved-changes.guard';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { AuthGuard } from './_guards/auth.guard';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { HomeComponent } from './home/home.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberListResolver } from './_resolvers/member-list.resolver';

const routes: Routes = [
  {path: '' , component: HomeComponent},
  {
    //protecting multiple routes with one guards
    path: '', 
    runGuardsAndResolvers: 'always',
    canActivate:[AuthGuard],
    children: [
      
      {path: 'members' , component: MemberListComponent, 
      resolve:{ users: MemberListResolver}},


      {path: 'members/:id' , component: MemberDetailComponent, 
      resolve: {user: MemberDetailResolver}},

      {path:'member/edit' , component:MemberEditComponent, 
      resolve: {user: MemberEditResolver},
    canDeactivate:[PreventUnsavedChanges]},

      {path: 'messages' , component: MessagesComponent},
      {path: 'lists' , component: ListsComponent},
    ]
  },
  
  //In the case of redirection
  {path: '**' , redirectTo: '', pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
