import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

//https://angular.dev/guide/forms/dynamic-forms
@Injectable({
  providedIn: 'root'
})
export class QuestionControlService {
  constructor(private fb: FormBuilder) {}
  toFormGroup(questions: QuestionBase<string>[]) {
    const group: any = {};
    questions.forEach((question) => {
      group[question.key] = this.fb.control<string | null>(question.value ?? null, { nonNullable: true });
    });
    return new FormGroup(group);
  }
}

export class QuestionBase<T> {
  value: T | undefined;
  key: string;
  //label: string;
  //required: boolean;
  //order: number;
  //controlType: string;
  //type: string;
  //options: {key: string; value: string}[];
  constructor(
    options: {
      value?: T;
      key?: string;
      //label?: string;
      //required?: boolean;
      //order?: number;
      //controlType?: string;
      //type?: string;
      //options?: {key: string; value: string}[];
    } = {},
  ) {
    this.value = options.value;
    this.key = options.key || '';
    //this.label = options.label || '';
    //this.required = !!options.required;
    //this.order = options.order === undefined ? 1 : options.order;
    //this.controlType = options.controlType || '';
    //this.type = options.type || '';
    //this.options = options.options || [];
  }
}