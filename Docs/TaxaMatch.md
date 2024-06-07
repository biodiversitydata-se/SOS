# Taxa match
A brief description of the SOS taxa match algorithm.

## Step 1 - Dyntaxa TaxonId match
1. Try to match `verbatimObservation.TaxonID`. It will match if the string is in this format: `urn:lsid:dyntaxa:{id}` and the id matches a valid Dyntaxa taxon.

## Step 2 - Name match
The following names are attempted to be matched in order.
1. ScientificName. Example: `Acanthodoris pilosa (Abildgaard, 1789)`
2. Species (when specified). Example: `Acanthodoris pilosa`
3. ScientificName without parenthesis values. Example: `Acanthodoris pilosa`

## Name match algorithm
1. Try match by Dyntaxa ScientificNames. Example value: `Cyathomonadaceae`
2. Try match by Dyntaxa ScientificNames + Author. Example value: `Cyathomonadaceae E.G.Pringsh.`
3. Try match by Dyntaxa ScientificNames - Author (when `verbatimObservation.scientificNameAuthorship` is specified). Example value: `Cyathomonadaceae`
4. Try step 1-3 for synonyms.