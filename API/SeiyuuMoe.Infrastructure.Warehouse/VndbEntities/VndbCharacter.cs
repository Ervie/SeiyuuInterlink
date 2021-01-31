﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeiyuuMoe.Infrastructure.Warehouse.VndbEntities
{
	[Table("chars")]
	public class VndbCharacter
	{
		public VndbCharacter()
		{
			VisualNovelSeiyuus = new HashSet<VndbVisualNovelSeiyuu>();
		}

		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("name")]
		public string Name { get; set; }

		[Column("original")]
		public string NameOriginal { get; set; }

		[Column("alias")]
		public string Alias { get; set; }

		[Column("image")]
		public string Image { get; set; }

		[Column("desc")]
		public string Description { get; set; }

		public virtual ICollection<VndbVisualNovelSeiyuu> VisualNovelSeiyuus { get; set; }
	}
}