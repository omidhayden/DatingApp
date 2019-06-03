import { AuthGuard } from './_guards/auth.guard';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { MemberListComponent } from './member-list/member-list.component';
import { HomeComponent } from './home/home.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
  {path: '' , component: HomeComponent},
  {
    //protecting multiple routes with one guards
    path: '', 
    runGuardsAndResolvers: 'always',
    canActivate:[AuthGuard],
    children: [
      {path: 'members' , component: MemberListComponent},
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
