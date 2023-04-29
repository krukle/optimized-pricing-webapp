import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { SkuComponent } from './sku/sku.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { SkuSearchComponent } from './sku-search/sku-search.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    SkuComponent,
    SkuSearchComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'sku', component: SkuSearchComponent },
      { path: 'sku/:id', component: SkuComponent },
      { path: '**', redirectTo: 'sku', pathMatch: 'full'}
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
