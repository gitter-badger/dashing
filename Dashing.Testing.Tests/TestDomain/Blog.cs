﻿namespace Dashing.Testing.Tests.TestDomain {
    using System;
    using System.Collections.Generic;

    public class Blog {
        public Blog() {
            this.Posts = new List<Post>();
        }

        public virtual int BlogId { get; set; }

        public virtual string Title { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual string Description { get; set; }

        public virtual User Owner { get; set; }

        public virtual IList<Post> Posts { get; set; }
    }
}