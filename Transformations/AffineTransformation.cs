﻿namespace CoordinateTransformations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CoordinateTransformations.Data;
    using CoordinateTransformations.Data.Enums;

    public class AffineTransformation
    {
        private readonly ICollection<CoordinateTransformations.Data.Point> points;

        private double a0;
        private double a1;
        private double a2;
        private double b0;
        private double b1;
        private double b2;

        public AffineTransformation(ICollection<CoordinateTransformations.Data.Point> points)
        {
            this.points = new List<CoordinateTransformations.Data.Point>(points);
        }

        public static int MinPointsCount
        {
            get
            {
                return 3;
            }
        }

        public int CommonPointsCount
        {
            get
            {
                int commonPointsCount = this.points.Count(p => p.PointType == PointType.CommonPoint);

                return commonPointsCount;
            }
        }

        public double B2
        {
            get
            {
                return this.b2;
            }
        }

        public double B1
        {
            get
            {
                return this.b1;
            }
        }

        public double B0
        {
            get
            {
                return this.b0;
            }
        }

        public double A2
        {
            get
            {
                return this.a2;
            }
        }

        public double A1
        {
            get
            {
                return this.a1;
            }
        }

        public double A0
        {
            get
            {
                return this.a0;
            }
        }

        public ICollection<Point> Points
        {
            get
            {
                return this.points;
            }
        }

        public void TransformCoordinates()
        {
            if (this.CommonPointsCount < AffineTransformation.MinPointsCount)
            {
                throw new ArgumentException(string.Format("Броят на общите точки в двете координатни системи [{0}] е по-малък от [{1}]!", this.CommonPointsCount, AffineTransformation.MinPointsCount));
            }

            double ax1 = this.points.Where(p => p.PointType == PointType.CommonPoint).Average(f => f.SourcePoint.PositionX);
            double ay1 = this.points.Where(p => p.PointType == PointType.CommonPoint).Average(f => f.SourcePoint.PositionY);
            double ax2 = this.points.Where(p => p.PointType == PointType.CommonPoint).Average(f => f.TargetPoint.PositionX);
            double ay2 = this.points.Where(p => p.PointType == PointType.CommonPoint).Average(f => f.TargetPoint.PositionY);

            double dx1, dy1, dx2, dy2;
            double p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0;
            foreach (var point in this.points.Where(p => p.PointType == PointType.CommonPoint))
            {
                dx1 = point.SourcePoint.PositionX - ax1;
                dy1 = point.SourcePoint.PositionY - ay1;
                dx2 = point.TargetPoint.PositionX - ax2;
                dy2 = point.TargetPoint.PositionY - ay2;
                p1 += dx1 * dx1;
                p2 += dy1 * dy1;
                p3 += dx1 * dx2;
                p4 += dx1 * dy2;
                p5 += dy1 * dx2;
                p6 += dy1 * dy2;
                p7 += dx1 * dy1;
            }

            double q1 = (p2 * p3) - (p7 * p5);
            double q2 = (p1 * p5) - (p7 * p3);
            double q3 = (p2 * p4) - (p7 * p6);
            double q4 = (p1 * p6) - (p7 * p4);
            double q5 = (p1 * p2) - (p7 * p7);

            this.a1 = q1 / q5;
            this.a2 = q2 / q5;
            this.b1 = q3 / q5;
            this.b2 = q4 / q5;
            this.a0 = ax2 - (this.a1 * ax1) - (this.a2 * ay1);
            this.b0 = ay2 - (this.b1 * ax1) - (this.b2 * ay1);

            foreach (var point in this.points)
            {
                double x = this.a0 + (this.a1 * point.SourcePoint.PositionX) + (this.a2 * point.SourcePoint.PositionY);
                double y = this.b0 + (this.b1 * point.SourcePoint.PositionX) + (this.b2 * point.SourcePoint.PositionY);

                switch (point.PointType)
                {
                    case PointType.CommonPoint:
                        double vx = (point.TargetPoint.PositionX - x) * 1000;
                        double vy = (point.TargetPoint.PositionY - y) * 1000;

                        point.OffsetX = vx;
                        point.OffsetY = vy;

                        break;
                    case PointType.NewPoint:
                        point.TargetPoint.PositionX = x;
                        point.TargetPoint.PositionY = y;

                        break;
                }
            }
        }
    }
}