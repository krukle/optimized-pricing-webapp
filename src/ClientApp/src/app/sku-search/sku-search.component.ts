import {Component} from '@angular/core';
import {Router} from "@angular/router";
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-sku-search',
  templateUrl: './sku-search.component.html',
  styleUrls: ['./sku-search.component.css']
})
export class SkuSearchComponent {

  sku = "";
  router: Router;

  constructor(router: Router) {
    this.router = router
  }

  // On submit. Direct to sku/{sku}.
  onSubmit(event: Event, sku: string) {
    event.preventDefault();
    console.log('SkuSearchComponent | onSubmit | data ' + sku);
    this.router.navigate(['/sku', sku]);
  }

}
