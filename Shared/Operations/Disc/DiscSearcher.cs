/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Classes.Disc;

namespace Archiver.Shared.Operations.Disc
{
    public class DiscSearcher
    {
        private IEnumerable<DiscSourceFile> _allFiles;

        public DiscSearcher(List<DiscDetail> discs)
        {
            if (discs is null)
                throw new ArgumentNullException(nameof(discs));

            _allFiles = discs.SelectMany(x => x.Files);
        }

        public async IAsyncEnumerable<DiscSourceFile> FindFilesAsync(
            string searchString, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            ) 
        {
            foreach (DiscSourceFile file in _allFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (file.RelativePath.ToLower().Contains(searchString))
                    yield return file;

                await Task.CompletedTask;
            }
        }
    }
}