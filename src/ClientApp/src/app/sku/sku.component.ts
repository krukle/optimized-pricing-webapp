import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-sku',
  templateUrl: './sku.component.html',
  styleUrls: ['./sku.component.css']
})
export class SkuComponent {
  public priceDetails: PriceDetail[] = [];


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private route: ActivatedRoute) {
    const id = this.route.snapshot.paramMap.get('id');
    http.get<PriceDetail[]>(baseUrl + 'pricedetail/' + id).subscribe(result => {
      this.priceDetails = result;
    }, error => console.error(error));
  }
}

interface PriceDetail {
  priceValueId: number;
  created: Date;
  modified: Date;
  catalogEntryCode: string;
  marketId: string;
  currencyCode: string;
  validFrom: Date;
  validUntil: Date;
  unitPrice: number;
}
