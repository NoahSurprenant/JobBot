import { ChangeDetectionStrategy, Component, effect, input, OnInit, resource, signal, WritableSignal } from '@angular/core';
import { ButtonComponent } from '../shared/button/button.component';
import { CommonModule } from '@angular/common';
import { PaginationResult } from '../paginationResult';
import { PaginatorComponent } from '../shared/paginator/paginator.component';
import { DropdownComponent } from "../shared/dropdown/dropdown.component";
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-jobs',
  imports: [
    CommonModule,
    ButtonComponent,
    PaginatorComponent,
    DropdownComponent,
    ReactiveFormsModule,
    RouterModule,
],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobsComponent implements OnInit {
  label = input<string | null>(null);
  formControl: FormControl<string | null> = new FormControl(null);
  appliedFilter: WritableSignal<boolean | null> = signal<boolean | null>(null);
  questionKind = input<'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine' | null>(null);
  
  constructor() {
    effect(() => {
      const x = this.x.value();
      if (x)
        this.current.set(x);
    })

    this.formControl.valueChanges.subscribe({
      next: (x) => {
        if (x != 'null') {
          this.appliedFilter.set(coerceBooleanProperty(x));
        }
        else {
          this.appliedFilter.set(null);
        }
      }
    })
  }

  isEven(i: number) {
    return i % 2 == 0;
  }

  ngOnInit(): void {
    this.x.reload();
  }

  clicked(): void {
    this.x.reload();
  }

  pageSize = signal(10);
  pageNumber = signal(1);

  x = resource({
    request: () => ({ pageSize: this.pageSize(),
                      pageNumber: this.pageNumber(),
                      label: this.label(),
                      questionKind: this.questionKind(),
                      appliedFilter: this.appliedFilter(),
                    }),
    loader: async ({request}) => {
      const params = new URLSearchParams();
      params.set('pageSize', request.pageSize.toString());
      params.set('pageNumber', request.pageNumber.toString());
      const obj: JobFilter = {
        label: request.label,
        questionKind: request.questionKind,
        applied: request.appliedFilter,
      };
      return await fetch(`api/jobs?${params}`, {
        method: 'POST',
        body: JSON.stringify(obj),
        headers: {
          "Content-Type": "application/json",
        },
      })
        .then(x => x.json() as Promise<PaginationResult<JobDto>>);
    },
  });

  current = signal<PaginationResult<JobDto>>({ totalCount: 0, results: []});
}

export interface JobDto
{
  jobID: number,
  companyName: string,
  companyLink: string | null,
  jobTitle: string,
  location: string,
  officeKind: 'Remote' | 'OnSite' | 'Hybrid' | 'Unknown',
  salaryMin: number | null,
  salaryMax: number | null,
  noApplyReason: string | null,
}

export interface JobFilter
{
  label: string | null,
  questionKind: 'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine' |null,
  applied: boolean | null,
}