﻿using System;
using OpenTK;

namespace ClassicalSharp.Particles {

	public sealed class TerrainParticle : Particle {
		
		public TextureRectangle Rectangle;
		const float gravity = 2.4f;
		double maxY = 0;
		
		public TerrainParticle( Game window, Vector3 pos, Vector3 velocity, double lifetime, TextureRectangle rec )
			: base( window, pos, velocity, lifetime ) {
			Rectangle = rec;
			maxY = Position.Y;
		}
		
		public override void Render( double delta, float t, VertexPos3fTex2f[] vertices, ref int index ) {
			Position = Vector3.Lerp( lastPos, nextPos, t );
			float x1 = Position.X, y1 = Position.Y, z1 = Position.Z,
			x2 = Position.X + Size.X, y2 = Position.Y + Size.Y;
			vertices[index++] = new VertexPos3fTex2f( x1, y1, z1, Rectangle.U1, Rectangle.V2 );
			vertices[index++] = new VertexPos3fTex2f( x1, y2, z1, Rectangle.U1, Rectangle.V1 );
			vertices[index++] = new VertexPos3fTex2f( x2, y2, z1, Rectangle.U2, Rectangle.V1 );
			
			vertices[index++] = new VertexPos3fTex2f( x1, y1, z1, Rectangle.U1, Rectangle.V2 );
			vertices[index++] = new VertexPos3fTex2f( x2, y1, z1, Rectangle.U2, Rectangle.V2 );
			vertices[index++] = new VertexPos3fTex2f( x2, y2, z1, Rectangle.U2, Rectangle.V1 );
		}

		public override bool Tick( double delta ) {
			lastPos = Position = nextPos;
			Velocity.Y -= gravity * (float)delta;
			int startY = (int)Math.Floor( Position.Y );
			Position += Velocity * (float)delta;
			int endY = (int)Math.Floor( Position.Y );			
			Utils.Clamp( ref Position.X, 0, Window.Map.Width - 0.01f );
			Utils.Clamp( ref Position.Z, 0, Window.Map.Length - 0.01f );
			
			if( endY <= startY ) {
				for( int y = startY; y >= endY; y-- ) {
					if( y < 0 ) {
						return CollideWithGround( 0 ) ? true : base.Tick( delta );
					}
					byte block = GetBlock( (int)Position.X, y, (int)Position.Z );
					if( block == 0 || Window.BlockInfo.IsSprite( block ) || Window.BlockInfo.IsLiquid( block ) )
						continue;
					
					float groundHeight = y + Window.BlockInfo.BlockHeight( block );				
					if( Position.Y < groundHeight ) {
						return CollideWithGround( groundHeight ) ? true : base.Tick( delta );
					}
				}
			}
			nextPos = Position;
			Position = lastPos;
			return base.Tick( delta );
		}
		
		byte GetBlock( int x, int y, int z ) {
			// If particles are spawned at the top of the map, they can occasionally
			// go outside the top of the map. This is okay, so handle this case.
			if( y >= Window.Map.Height ) return 0;
			return Window.Map.GetBlock( x, y, z );
		}
		
		bool CollideWithGround( float y ) {
			if( y > maxY ) {
				// prevent the particle teleporting up when a block is 
				// placed on top of the particle, simply die instead.
				return true;
			}
			Position.Y = y;
			maxY = y;
			Velocity = Vector3.Zero;
			nextPos = Position;
			Position = lastPos;
			return false;
		}
		
		public override void Dispose() {
		}
	}
}
