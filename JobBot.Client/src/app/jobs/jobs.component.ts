import { ChangeDetectionStrategy, Component, effect, OnInit, resource, signal } from '@angular/core';
import { ButtonComponent } from '../shared/button/button.component';
import { CommonModule } from '@angular/common';
import { PaginationResult } from '../paginationResult';
import { PaginatorComponent } from '../shared/paginator/paginator.component';

@Component({
  selector: 'app-jobs',
  imports: [
    CommonModule,
    ButtonComponent,
    PaginatorComponent,
  ],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobsComponent implements OnInit {
  constructor() {
    effect(() => {
      const x = this.x.value();
      if (x)
        this.current.set(x);
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
    request: () => ({ pageSize: this.pageSize(), pageNumber: this.pageNumber() }),
    loader: async ({request}) => {
      const params = new URLSearchParams();
      params.set('pageSize', request.pageSize.toString());
      params.set('pageNumber', request.pageNumber.toString());
      return await fetch(`api/jobs?${params}`)
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
