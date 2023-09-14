<StyledLayerDescriptor version="1.0.0"
    xsi:schemaLocation="http://www.opengis.net/sld StyledLayerDescriptor.xsd"
    xmlns="http://www.opengis.net/sld"
    xmlns:ogc="http://www.opengis.net/ogc"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <NamedLayer>
    <Name>GeoHashGrid</Name>
    <UserStyle>
      <Title>GeoHashGrid</Title>
      <Abstract>GeoHashGrid aggregation</Abstract>
      <FeatureTypeStyle>
        <Transformation>
          <ogc:Function name="vec:GeoHashGrid">
            <ogc:Function name="parameter">
              <ogc:Literal>data</ogc:Literal>
            </ogc:Function>
            <ogc:Function name="parameter">
              <ogc:Literal>gridStrategy</ogc:Literal>
              <ogc:Literal>Basic</ogc:Literal>
            </ogc:Function>
            <ogc:Function name="parameter">
              <ogc:Literal>outputBBOX</ogc:Literal>
              <ogc:Function name="env">
                <ogc:Literal>wms_bbox</ogc:Literal>
              </ogc:Function>
            </ogc:Function>
            <ogc:Function name="parameter">
              <ogc:Literal>outputWidth</ogc:Literal>
              <ogc:Function name="env">
                <ogc:Literal>wms_width</ogc:Literal>
              </ogc:Function>
            </ogc:Function>
            <ogc:Function name="parameter">
              <ogc:Literal>outputHeight</ogc:Literal>
              <ogc:Function name="env">
                <ogc:Literal>wms_height</ogc:Literal>
              </ogc:Function>
            </ogc:Function>
          </ogc:Function>
        </Transformation>
        <Rule>
         <RasterSymbolizer>
           <Geometry>
             <!-- Actual geometry property name in feature source -->
             <ogc:PropertyName>location.pointLocation</ogc:PropertyName></Geometry>
           <Opacity>0.6</Opacity>
           <ColorMap type="ramp" >
             <ColorMapEntry color="#FFFFFF" quantity="0" label="nodata" opacity="0"/>
             <ColorMapEntry color="#2851CC" quantity="1" label="values"/>
             <ColorMapEntry color="#211F1F" quantity="2" label="label"/>
             <ColorMapEntry color="#EE0F0F" quantity="3" label="label"/>
             <ColorMapEntry color="#AAAAAA" quantity="4" label="label"/>
             <ColorMapEntry color="#6FEE4F" quantity="5" label="label"/>
             <ColorMapEntry color="#DDB02C" quantity="10" label="label"/>
           </ColorMap>
         </RasterSymbolizer>
        </Rule>
      </FeatureTypeStyle>
    </UserStyle>
  </NamedLayer>
 </StyledLayerDescriptor>